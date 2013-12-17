namespace EzBob.Backend.Strategies
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using PaymentServices.PayPoint;
	using Web.Code;
	using log4net;
	using DbConnection;

	public class AutoPaymentResult
	{
		public decimal ActualAmountCharged { get; set; }
		public bool PaymentFailed { get; set; }
		public bool PaymentFailedBy1Day { get; set; }
		public bool PaymentCollectedSuccessfully { get; set; }
		public bool IsException { get; set; }
	}

	public class PayPointCharger
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(PayPointCharger));
		private readonly StrategiesMailer mailer = new StrategiesMailer();
		private readonly PayPointApi payPointApi = new PayPointApi();
		
		public PayPointCharger()
		{
			ReadConfigs();
		}

		private int collectionPeriod1;
		private int collectionPeriod2;
		private int amountToChargeFrom;


		private void ReadConfigs()
		{
			DataTable configsDataTable = DbConnection.ExecuteSpReader("PayPointChargerGetConfigs");
			DataRow configsResult = configsDataTable.Rows[0];

			collectionPeriod1 = int.Parse(configsResult["CollectionPeriod1"].ToString());
			collectionPeriod2 = int.Parse(configsResult["CollectionPeriod2"].ToString());
			amountToChargeFrom = int.Parse(configsResult["AmountToChargeFrom"].ToString());
		}

		public void Execute()
		{
			DataTable dt = DbConnection.ExecuteSpReader("GetCustomersForPayPoint");
			foreach (DataRow row in dt.Rows)
			{
				HandleOnePayment(row);
			}
		}

		private void HandleOnePayment(DataRow row)
		{
			int loanScheduleId = int.Parse(row["id"].ToString());
			int loanId = int.Parse(row["LoanId"].ToString());
			string firstName = row["FirstName"].ToString();
			int customerId = int.Parse(row["CustomerId"].ToString());
			string customerMail = row["Email"].ToString();
			string fullname = row["Fullname"].ToString();
			DateTime scheduledDate = DateTime.Parse(row["Date"].ToString());
			bool reductionFee = bool.Parse(row["ReductionFee"].ToString());
			bool latePaymentNotification;
			if (!bool.TryParse(row["LatePaymentNotification"].ToString(), out latePaymentNotification))
			{
				latePaymentNotification = true;
			}
			string refNum = row["RefNum"].ToString();
			bool lastInstallment = bool.Parse(row["LastInstallment"].ToString());

			decimal amountDue = payPointApi.GetAmountToPay(loanScheduleId);

			if (!ShouldCharge(lastInstallment, amountDue))
			{
				log.InfoFormat("Will not charge loan schedule id:{0} (The amount was:{1}). The minimal amount for collection is:{2}", loanScheduleId, amountDue, amountToChargeFrom);
				return;
			}

			decimal initialAmountDue = amountDue;

			AutoPaymentResult autoPaymentResult = TryToMakeAutoPayment(loanScheduleId, initialAmountDue, customerId, customerMail,
				fullname, scheduledDate, reductionFee);

			if (autoPaymentResult.IsException)
			{
				return;
			}

			if (autoPaymentResult.PaymentFailed)
			{
				SendFailureMail(firstName, initialAmountDue, customerMail);
				return;
			}

			if (autoPaymentResult.PaymentFailedBy1Day)
			{
				if ((scheduledDate <= DateTime.UtcNow &&
				     scheduledDate.AddDays(collectionPeriod1) >= DateTime.UtcNow &&
				     latePaymentNotification
					) ||
					(scheduledDate.AddDays(collectionPeriod1 + 1) <= DateTime.UtcNow && 
					 scheduledDate.AddDays(collectionPeriod2) >= DateTime.UtcNow &&
					 latePaymentNotification)
					)
				{
					var variables = new Dictionary<string, string>
						{
							{"AmountCharged", initialAmountDue.ToString(CultureInfo.InvariantCulture)},
							{"RefNum", refNum},
							{"FirstName", firstName}
						};
					mailer.SendToCustomerAndEzbob(variables, customerMail, "Mandrill - Loan repayment late (1D late)", "Your re-payment is late");

					SendConfirmationMail(firstName, amountDue, refNum, customerMail);
				}

				SendLoanStatusMail(loanId, firstName, refNum, customerMail);
				return;
			}

			if (autoPaymentResult.PaymentCollectedSuccessfully)
			{
				SendConfirmationMail(firstName, amountDue, refNum, customerMail);
				SendLoanStatusMail(loanId, firstName, refNum, customerMail);
			}
		}

		private AutoPaymentResult TryToMakeAutoPayment(int loanScheduleId, decimal initialAmountDue, int customerId, 
			string customerMail, string fullname, DateTime scheduledDate, bool reductionFee)
		{
			var result = new AutoPaymentResult();
			decimal actualAmountCharged = initialAmountDue;

			int counter = 0;
			while (counter <= 3)
			{
				PayPointReturnData payPointReturnData;
				if (MakeAutoPayment(loanScheduleId, actualAmountCharged, out payPointReturnData))
				{
					if (IsNotEnoughMoney(payPointReturnData))
					{
						if (!reductionFee)
						{
							result.PaymentFailed = true;
							return result;
						}

						counter++;

						if (counter > 3)
						{
							result.PaymentFailed = true;
							return result;
						}

						if (counter == 1)
						{
							actualAmountCharged = Math.Round((initialAmountDue * (decimal)0.75), 2);
						}
						else if (counter == 2)
						{
							actualAmountCharged = Math.Round((initialAmountDue * (decimal)0.5), 2);
						}
						else if (counter == 3)
						{
							actualAmountCharged = Math.Round((initialAmountDue * (decimal)0.25), 2);
						}
					}
					else if (IsCollectionSuccessful(payPointReturnData))
					{
						result.PaymentCollectedSuccessfully = true;
						result.ActualAmountCharged = actualAmountCharged;
						return result;
					}
					else
					{
						if ((DateTime.UtcNow - scheduledDate).TotalDays >= 1)
						{
							result.PaymentFailedBy1Day = true;
							return result;
						}

						result.PaymentFailed = true;
						return result;
					}
				}
				else
				{
					SendExceptionMail(initialAmountDue, customerId, customerMail, fullname);
					result.IsException = true;
					return result;
				}
			}

			return result;
		}

		private void SendConfirmationMail(string firstName, decimal amountDue, string refNum, string customerMail)
		{
			string subject = string.Format("Dear {0}, your payment of £{1} has been credited to your ezbob account.", firstName, amountDue);
			var variables = new Dictionary<string, string>
				{
					{"AMOUNT", amountDue.ToString(CultureInfo.InvariantCulture)},
					{"FirstName", firstName},
					{"DATE", FormattingUtils.FormatDateToString(DateTime.UtcNow)},
					{"RefNum", refNum}
				};
			mailer.SendToCustomerAndEzbob(variables, customerMail, "Mandrill - Repayment confirmation", subject);
		}

		private void SendLoanStatusMail(int loanId, string firstName, string refNum, string customerMail)
		{
			DataTable dt = DbConnection.ExecuteSpReader("GetLoanStatus",
				DbConnection.CreateParam("LoanId", loanId));
			DataRow result = dt.Rows[0];

			string loanStatus = result["Status"].ToString();
			if (loanStatus == "PaidOff")
			{
				var variables = new Dictionary<string, string>
					{
						{"FirstName", firstName},
						{"RefNum", refNum}
					};

				mailer.SendToCustomerAndEzbob(variables, customerMail, "Mandrill - Loan paid in full", "You have paid your loan off in full.  Benefit from a lower interest cost on your next loan.");
			}
		}

		private void SendFailureMail(string firstName, decimal initialAmountDue, string customerMail)
		{
			var variables = new Dictionary<string, string>
				{
					{"FirstName", firstName},
					{"AmountOwed", initialAmountDue.ToString(CultureInfo.InvariantCulture)},
					{"DueDate", FormattingUtils.FormatDateToString(DateTime.UtcNow)}
				};

			mailer.SendToCustomerAndEzbob(variables, customerMail, "Mandrill - Automatic Re-Payment has Failed", "Your Automatic Re-Payment has Failed");
		}

		private void SendExceptionMail(decimal initialAmountDue, int customerId, string customerMail, string fullName)
		{
			var variables = new Dictionary<string, string>
				{
					{"UserID", customerId.ToString(CultureInfo.InvariantCulture)},
					{"Email", customerMail},
					{"FullName", fullName},
					{"Amount", initialAmountDue.ToString(CultureInfo.InvariantCulture)}
				};

			mailer.SendToEzbob(variables, "Mandrill - PayPoint Script Exception", "Script node crash");
		}

		private static bool IsCollectionSuccessful(PayPointReturnData payPointReturnData)
		{
			return payPointReturnData.Code == "A";
		}

		private static bool IsNotEnoughMoney(PayPointReturnData payPointReturnData)
		{
			return payPointReturnData.Code == "P:A";
		}

		private bool ShouldCharge(bool lastInstallment, decimal amountDue)
		{
			return (lastInstallment && amountDue > 0) || amountDue >= amountToChargeFrom;
		}

		private bool MakeAutoPayment(int loanScheduleId, decimal amountDue, out PayPointReturnData result)
		{
			try
			{
				result = payPointApi.MakeAutomaticPayment(loanScheduleId, amountDue);
				return true;
			}
			catch (Exception ex)
			{
				log.ErrorFormat("Failed making auto payment for loan schedule id:{0} exception:{1}", loanScheduleId, ex);
				result = null;
				return false;
			}
		}
	}
}
