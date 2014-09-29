namespace EzBob.Backend.Strategies.Misc {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using Ezbob.Database;
	using Ezbob.Logger;
	using MailStrategies.API;
	using PaymentServices.PayPoint;

	public class SetLateLoanStatus : AStrategy {
		#region public

		#region constructor

		public SetLateLoanStatus(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			mailer = new StrategiesMailer(DB, Log);

			DataTable configsDataTable = DB.ExecuteReader("SetLateLoanStatusGetConfigs", CommandSpecies.StoredProcedure);
			var sr = new SafeReader(configsDataTable.Rows[0]);

			collectionPeriod1 = sr["CollectionPeriod1"];
			collectionPeriod2 = sr["CollectionPeriod2"];
			collectionPeriod3 = sr["CollectionPeriod3"];
			latePaymentCharge = sr["LatePaymentCharge"];
			latePaymentChargeId = sr["LatePaymentChargeId"];
			partialPaymentCharge = sr["PartialPaymentCharge"];
			partialPaymentChargeId = sr["PartialPaymentChargeId"];
			administrationCharge = sr["AdministrationCharge"];
			administrationChargeId = sr["AdministrationChargeId"];
			amountToChargeFrom = sr["AmountToChargeFrom"];

			loanIdPrev = -1;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Set Late Loan Status"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			MarkLoansAsLate();

			DataTable lateForCollectionDataTable = DB.ExecuteReader("GetLateForCollection", CommandSpecies.StoredProcedure);

			foreach (DataRow row in lateForCollectionDataTable.Rows) {
				var sr = new SafeReader(row);
				DateTime date = sr["Date"];
				decimal amountDue = sr["AmountDue"];
				int loanId = sr["LoanId"];
				int loanScheduleId = sr["LoanScheduleId"];
				decimal interest = sr["Interest"];
				string mail = sr["email"];
				string firstName = sr["FirstName"];
				string refNum = sr["RefNum"];
				DateTime customInstallmentDate = sr["CustomInstallmentDate"];
				bool sentLastNotice = sr["LastNoticeSent"];

				if (customInstallmentDate != default(DateTime)) {
					if (date < customInstallmentDate)
						date = customInstallmentDate;
				} // if

				int daysBetween = (int)(DateTime.UtcNow - date).TotalDays;
				int feeAmount, feeType;
				CalculateFee(daysBetween, interest, out feeAmount, out feeType);

				bool appliedLateCharge = false;
				if (feeType != 0)
				{
					var papi = new PayPointApi();
					appliedLateCharge = papi.ApplyLateCharge(feeAmount, loanId, feeType);
				} // if

				var safeReader = DB.GetFirst("ShouldStopSendingLateMails", new QueryParameter("LoanId", loanId));
				bool stopSendingEmails = safeReader["StopSendingEmails"];
				string templateName = null;

				if (!stopSendingEmails)
				{
					if (daysBetween < collectionPeriod1) // 7
					{
						templateName = "Mandrill - ezbob - you missed your payment";
					}
					else if (daysBetween < collectionPeriod2) // 14
					{
						templateName = appliedLateCharge ? "Mandrill - ezbob - £20 late fee was added to your account" : "Mandrill - ezbob - Warning - you missed your payment for more than 7 days";
					}
					else if (daysBetween < collectionPeriod3) // 30
					{
						templateName = appliedLateCharge ? "Mandrill - Warning notice - ezbob - £40 late fee was added" : "Mandrill - Warning notice - ezbob - you missed your payment for more than 14 days";
					}
					else if (!sentLastNotice)
					{
						templateName = "Mandrill - ezbob - Last warning - Debt recovery agency";

						DB.ExecuteNonQuery(
							"UpdateLastNotice",
							CommandSpecies.StoredProcedure,
							new QueryParameter("LoanScheduleId", loanScheduleId));
					}
					else
					{
						Log.Info("Processed late scheduled payment id:{0}, since last notice was already sent nothing was done.", loanScheduleId);
					}

					if (templateName != null)
					{
						var variables = new Dictionary<string, string>
							{
								{"FirstName", firstName},
								{"ScheduledAmount", amountDue.ToString(CultureInfo.InvariantCulture)},
								{"RefNum", refNum},
								{"FeeAmount", feeAmount.ToString(CultureInfo.InvariantCulture)},
								{"AmountCharged", amountDue.ToString(CultureInfo.InvariantCulture)}
							};
						mailer.Send(templateName, variables, new Addressee(mail));
					}
				}

				AccumulateFee(loanId, daysBetween, amountDue);
				
				DB.ExecuteNonQuery(
					"UpdateCollection",
					CommandSpecies.StoredProcedure,
					new QueryParameter("LoanId", loanId),
					new QueryParameter("Late30", late30),
					new QueryParameter("Late30Num", late30Num),
					new QueryParameter("Late60", late60),
					new QueryParameter("Late60Num", late60Num),
					new QueryParameter("Late90", late90),
					new QueryParameter("Late90Num", late90Num),
					new QueryParameter("PastDues", pastDues),
					new QueryParameter("PastDuesNum", pastDuesNum),
					new QueryParameter("IsDefaulted", 0),
					new QueryParameter("Late90Plus", late90Plus),
					new QueryParameter("Late90PlusNum", late90PlusNum)
				);
				
				loanIdPrev = loanId;
			} // foreach
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		#region method AccumulateFee

		private void AccumulateFee(int loanId, int daysBetween, decimal amountDue) {
			if (loanId != loanIdPrev) {
				late30 = 0;
				late60 = 0;
				late90 = 0;
				late30Num = 0;
				late60Num = 0;
				late90Num = 0;
				pastDues = 0;
				pastDuesNum = 0;
				late90Plus = 0;
				late90PlusNum = 0;
			} // if

			if (loanIdPrev == -1)
				loanIdPrev = loanId;

			if (daysBetween > 0 && daysBetween <= 30) {
				late30Num++;
				late30 += amountDue;
			}
			else if (daysBetween > 30 && daysBetween <= 60) {
				late60Num++;
				late60 += amountDue;
			}
			else if (daysBetween > 60 && daysBetween <= 90) {
				late90Num++;
				late90 += amountDue;
			}
			else { // daysBetween > 90
				late90PlusNum++;
				late90Plus += amountDue;
			} // if

			pastDues += amountDue;
			pastDuesNum++;
		} // AccumulateFee

		#endregion method AccumulateFee

		#region method CalculateFee

		private void CalculateFee(int daysBetween, decimal interest, out int feeAmount, out int feeType) {
			feeAmount = 0;
			feeType = 0;

			if (daysBetween >= collectionPeriod1 && daysBetween < collectionPeriod2) {
				feeAmount = latePaymentCharge;
				feeType = latePaymentChargeId;
			}
			else if (daysBetween >= collectionPeriod2 && daysBetween < collectionPeriod3 && interest > 0) {
				feeAmount = administrationCharge;
				feeType = administrationChargeId;
			}
			else if (daysBetween >= collectionPeriod2 && daysBetween < collectionPeriod3 && interest <= 0) {
				feeAmount = partialPaymentCharge;
				feeType = partialPaymentChargeId;
			}
		} // CalculateFee

		#endregion method CalculateFee

		#region method MarkLoansAsLate

		private void MarkLoansAsLate() {
			DataTable loansToCollectDataTable = DB.ExecuteReader("GetLoansToCollect", CommandSpecies.StoredProcedure);

			foreach (DataRow row in loansToCollectDataTable.Rows) {
				var sr = new SafeReader(row);
				int id = sr["id"];
				int loanId = sr["LoanId"];
				bool isLastInstallment = sr["LastInstallment"];
				int customerId = sr["CustomerId"];
				DateTime customInstallmentDate = sr["CustomInstallmentDate"];

				if (!isLastInstallment) {
					decimal amountDue = new PayPointApi().GetAmountToPay(id);

					int daysBetweenCustom = (int) (customInstallmentDate - DateTime.UtcNow).TotalDays;

					if (!(amountDue > amountToChargeFrom && daysBetweenCustom > 1)) {
						DB.ExecuteNonQuery(
							"UpdateLoanScheduleCustomDate",
							CommandSpecies.StoredProcedure,
							new QueryParameter("Id", id)
						);
						continue;
					}
				} // if

				DB.ExecuteNonQuery(
					"UpdateLoanStatusToLate",
					CommandSpecies.StoredProcedure,
					new QueryParameter("LoanId", loanId),
					new QueryParameter("CustomerId", customerId),
					new QueryParameter("PaymentStatus", "Late"),
					new QueryParameter("LoanStatus", "Late")
				);

				DB.ExecuteNonQuery("UpdateLoanScheduleStatus",
					CommandSpecies.StoredProcedure,
					new QueryParameter("Id", id),
					new QueryParameter("Status", "Late")
				);
			} // foreach
		} // MarkLoansAsLate

		#endregion method MarkLoansAsLate

		#region properties

		private readonly StrategiesMailer mailer;
		private readonly int collectionPeriod1;
		private readonly int collectionPeriod2;
		private readonly int collectionPeriod3;
		private readonly int latePaymentCharge;
		private readonly int latePaymentChargeId;
		private readonly int partialPaymentCharge;
		private readonly int partialPaymentChargeId;
		private readonly int administrationCharge;
		private readonly int administrationChargeId;
		private readonly int amountToChargeFrom;
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

		#endregion properties

		#endregion private
	} // class 
} // namespace
