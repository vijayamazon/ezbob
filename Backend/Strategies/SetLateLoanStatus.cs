namespace EzBob.Backend.Strategies {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using Ezbob.Database;
	using Ezbob.Logger;
	using PaymentServices.PayPoint;

	public class SetLateLoanStatus : AStrategy {
		#region public

		#region constructor

		public SetLateLoanStatus(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			mailer = new StrategiesMailer(DB, Log);

			DataTable configsDataTable = DB.ExecuteReader("SetLateLoanStatusGetConfigs", CommandSpecies.StoredProcedure);
			var sr = new SafeReader(configsDataTable.Rows[0]);

			collectionPeriod1 = sr.Int("CollectionPeriod1");
			collectionPeriod2 = sr.Int("CollectionPeriod2");
			collectionPeriod3 = sr.Int("CollectionPeriod3");
			latePaymentCharge = sr.Int("LatePaymentCharge");
			latePaymentChargeId = sr.Int("LatePaymentChargeId");
			partialPaymentCharge = sr.Int("PartialPaymentCharge");
			partialPaymentChargeId = sr.Int("PartialPaymentChargeId");
			administrationCharge = sr.Int("AdministrationCharge");
			administrationChargeId = sr.Int("AdministrationChargeId");
			amountToChargeFrom = sr.Int("AmountToChargeFrom");

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
				DateTime date = sr.DateTime("Date");
				decimal amountDue = sr.Decimal("AmountDue");
				int loanId = sr.Int("LoanId");
				decimal interest = sr.Decimal("Interest");
				string mail = sr.String("email");
				string firstName = sr.String("FirstName");
				string refNum = sr.String("RefNum");
				DateTime customInstallmentDate = sr.DateTime("CustomInstallmentDate");

				if (customInstallmentDate != default(DateTime)) {
					if (date < customInstallmentDate)
						date = customInstallmentDate;
				} // if

				int daysBetween = (int)(DateTime.UtcNow - date).TotalDays;
				int feeAmount, feeType;
				CalculateFee(daysBetween, interest, out feeAmount, out feeType);

				if (feeType != 0) {
					var papi = new PayPointApi();
					bool applyLateCharge = papi.ApplyLateCharge(feeAmount, loanId, feeType);

					if (applyLateCharge) {
						string subject = string.Format("Dear {0}, your payment of £{1} is {2} days past due. You will be charged a late fee", firstName, amountDue, daysBetween);
						string templateName = feeAmount >= partialPaymentCharge ? "Mandrill - Late fee was added (7D late)" : "Mandrill - Late fee was added (14D late)";

						var variables = new Dictionary<string, string> {
							{"FirstName", firstName},
							{"ScheduledAmount", amountDue.ToString(CultureInfo.InvariantCulture)},
							{"RefNum", refNum},
							{"FeeAmount", feeAmount.ToString(CultureInfo.InvariantCulture)}
						};

						mailer.SendToCustomerAndEzbob(variables, mail, templateName, subject);
					} // if
				} // if

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
				int id = sr.Int("id");
				int loanId = sr.Int("LoanId");
				bool isLastInstallment = sr.Bool("LastInstallment");
				int customerId = sr.Int("CustomerId");
				DateTime customInstallmentDate = sr.DateTime("CustomInstallmentDate");

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

		#endregion properties

		#endregion private
	} // class 
} // namespace
