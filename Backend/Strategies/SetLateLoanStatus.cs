namespace EzBob.Backend.Strategies
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using DbConnection;
	using PaymentServices.PayPoint;
	using log4net;

	public class SetLateLoanStatus
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(SetLateLoanStatus));
		private readonly StrategiesMailer mailer = new StrategiesMailer();
		private int collectionPeriod1;
		private int collectionPeriod2;
		private int collectionPeriod3;
		private int latePaymentCharge;
		private int latePaymentChargeId;
		private int partialPaymentCharge;
		private int partialPaymentChargeId;
		private int administrationCharge;
		private int administrationChargeId;
		private int amountToChargeFrom;
		private int loanIdPrev;
		private decimal late30;
		private decimal late60;
		private decimal late90;
		private decimal late90Plus;
		private decimal pastDues;
		private int late30Num;
		private int late60Num;
		private int late90Num;
		private int late90PlusNum;
		private int pastDuesNum;

		public SetLateLoanStatus()
		{
			ReadConfigs();
			loanIdPrev = -1;
		}

		private void ReadConfigs()
		{
			DataTable configsDataTable = DbConnection.ExecuteSpReader("SetLateLoanStatusGetConfigs");
			DataRow configsResult = configsDataTable.Rows[0];

			collectionPeriod1 = int.Parse(configsResult["CollectionPeriod1"].ToString());
			collectionPeriod2 = int.Parse(configsResult["CollectionPeriod2"].ToString());
			collectionPeriod3 = int.Parse(configsResult["CollectionPeriod3"].ToString());
			latePaymentCharge = int.Parse(configsResult["LatePaymentCharge"].ToString());
			latePaymentChargeId = int.Parse(configsResult["LatePaymentChargeId"].ToString());
			partialPaymentCharge = int.Parse(configsResult["PartialPaymentCharge"].ToString());
			partialPaymentChargeId = int.Parse(configsResult["PartialPaymentChargeId"].ToString());
			administrationCharge = int.Parse(configsResult["AdministrationCharge"].ToString());
			administrationChargeId = int.Parse(configsResult["AdministrationChargeId"].ToString());
			amountToChargeFrom = int.Parse(configsResult["AmountToChargeFrom"].ToString());
		}

		public void Execute()
		{
			MarkLoansAsLate();

			DataTable lateForCollectionDataTable = DbConnection.ExecuteSpReader("GetLateForCollection");
			foreach (DataRow row in lateForCollectionDataTable.Rows)
			{
				DateTime date = DateTime.Parse(row["Date"].ToString());
				decimal amountDue = decimal.Parse(row["AmountDue"].ToString());
				int loanId = int.Parse(row["LoanId"].ToString());
				decimal interest = decimal.Parse(row["Interest"].ToString());
				string mail = row["email"].ToString();
				string firstName = row["FirstName"].ToString();
				string refNum = row["RefNum"].ToString();
				DateTime customInstallmentDate;
				if (DateTime.TryParse(row["CustomInstallmentDate"].ToString(), out customInstallmentDate))
				{
					if (date < customInstallmentDate)
					{
						date = customInstallmentDate;
					}
				}

				int daysBetween = (int)(DateTime.UtcNow - date).TotalDays;
				int feeAmount, feeType;
				CalculateFee(daysBetween, interest, out feeAmount, out feeType);

				if (feeType != 0)
				{
					var papi = new PayPointApi();
					bool applyLateCharge = papi.ApplyLateCharge(feeAmount, loanId, feeType);
					if (applyLateCharge)
					{
						string subject = string.Format("Dear {0}, your payment of £{1} is {2} days past due. You will be charged a late fee", firstName, amountDue, daysBetween);
						string templateName = feeAmount >= partialPaymentCharge ? "Mandrill - Late fee was added (7D late)" : "Mandrill - Late fee was added (14D late)";

						var variables = new Dictionary<string, string>
							{
								{"FirstName", firstName},
								{"ScheduledAmount", amountDue.ToString(CultureInfo.InvariantCulture)},
								{"RefNum", refNum},
								{"FeeAmount", feeAmount.ToString(CultureInfo.InvariantCulture)}
							};

						mailer.SendToCustomerAndEzbob(variables, mail, templateName, subject);
					}
				}

				AccumulateFee(loanId, daysBetween, amountDue);
				
				DbConnection.ExecuteSpNonQuery("UpdateCollection",
				    DbConnection.CreateParam("LoanId", loanId),
				    DbConnection.CreateParam("Late30", late30),
					DbConnection.CreateParam("Late30Num", late30Num),
					DbConnection.CreateParam("Late60", late60),
					DbConnection.CreateParam("Late60Num", late60Num),
					DbConnection.CreateParam("Late90", late90),
					DbConnection.CreateParam("Late90Num", late90Num),
				    DbConnection.CreateParam("PastDues", pastDues),
				    DbConnection.CreateParam("PastDuesNum", pastDuesNum),
				    DbConnection.CreateParam("IsDefaulted", 0),
				    DbConnection.CreateParam("Late90Plus", late90Plus),
					DbConnection.CreateParam("Late90PlusNum", late90PlusNum));
				
				loanIdPrev = loanId;
			}
		}

		private void AccumulateFee(int loanId, int daysBetween, decimal amountDue)
		{
			if (loanId != loanIdPrev)
			{
				late30 = 0;
				late60 = 0;
				late90 = 0;
				late30Num = 0;
				late60Num = 0;
				late90Num = 0;
				pastDues = 0;
				pastDuesNum = 0;
			}

			if (loanIdPrev == -1)
			{
				loanIdPrev = loanId;
			}

			if (daysBetween > 0 && daysBetween <= 30)
			{
				late30Num++;
				late30 += amountDue;
			}
			else if (daysBetween > 30 && daysBetween <= 60)
			{
				late60Num++;
				late60 += amountDue;
			}
			else if (daysBetween > 60 && daysBetween <= 90)
			{
				late90Num++;
				late90 += amountDue;
			}
			else // daysBetween > 90
			{
				late90PlusNum++;
				late90Plus += amountDue;
			}
			pastDues += amountDue;
			pastDuesNum++;
		}

		private void CalculateFee(int daysBetween, decimal interest, out int feeAmount, out int feeType)
		{
			feeAmount = 0;
			feeType = 0;

			if (daysBetween >= collectionPeriod1 && daysBetween < collectionPeriod2)
			{
				feeAmount = latePaymentCharge;
				feeType = latePaymentChargeId;
			}
			else if (daysBetween >= collectionPeriod2 && daysBetween < collectionPeriod3 && interest > 0)
			{
				feeAmount = administrationCharge;
				feeType = administrationChargeId;
			}
			else if (daysBetween >= collectionPeriod2 && daysBetween < collectionPeriod3 && interest <= 0)
			{
				feeAmount = partialPaymentCharge;
				feeType = partialPaymentChargeId;
			}
		}

		private void MarkLoansAsLate()
		{
			DataTable loansToCollectDataTable = DbConnection.ExecuteSpReader("GetLoansToCollect");
			foreach (DataRow row in loansToCollectDataTable.Rows)
			{
				int id = int.Parse(row["id"].ToString());
				int loanId = int.Parse(row["LoanId"].ToString());
				bool isLastInstallment = bool.Parse(row["LastInstallment"].ToString());
				int customerId = int.Parse(row["CustomerId"].ToString());
				DateTime customInstallmentDate = DateTime.Parse(row["CustomInstallmentDate"].ToString());

				if (!isLastInstallment)
				{
					decimal amountDue = new PayPointApi().GetAmountToPay(id);

					int daysBetweenCustom = (int) (customInstallmentDate - DateTime.UtcNow).TotalDays;
					if (!(amountDue > amountToChargeFrom && daysBetweenCustom > 1))
					{
						DbConnection.ExecuteSpNonQuery("UpdateLoanScheduleCustomDate", DbConnection.CreateParam("Id", id));
						continue;
					}
				}

				DbConnection.ExecuteSpNonQuery("UpdateLoanStatusToLate",
				                               DbConnection.CreateParam("LoanId", loanId),
				                               DbConnection.CreateParam("CustomerId", customerId),
				                               DbConnection.CreateParam("PaymentStatus", "Late"),
				                               DbConnection.CreateParam("LoanStatus", "Late"));
				DbConnection.ExecuteSpNonQuery("UpdateLoanScheduleStatus",
				                               DbConnection.CreateParam("Id", id),
				                               DbConnection.CreateParam("Status", "Late"));
			}
		}
	}
}
