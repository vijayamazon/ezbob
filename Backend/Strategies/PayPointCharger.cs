namespace EzBob.Backend.Strategies {
	using MailStrategies.API;
	using Web.Code;
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using Ezbob.Database;
	using Ezbob.Logger;
	using PaymentServices.PayPoint;

	#region class AutoPaymentResult

	public class AutoPaymentResult {
		public decimal ActualAmountCharged { get; set; }
		public bool PaymentFailed { get; set; }
		public bool PaymentCollectedSuccessfully { get; set; }
		public bool IsException { get; set; }
	} // AutoPaymentResult

	#endregion class AutoPaymentResult

	#region class PayPointCharger

	public class PayPointCharger : AStrategy {
		#region public

		#region constructor

		public PayPointCharger(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			mailer = new StrategiesMailer(DB, Log);

			DataTable configsDataTable = DB.ExecuteReader("PayPointChargerGetConfigs", CommandSpecies.StoredProcedure);
			var sr = new SafeReader(configsDataTable.Rows[0]);
			amountToChargeFrom = sr["AmountToChargeFrom"];
		} // constructor

		#endregion constructor

		#region method Execute

		public override void Execute() {
			DataTable dt = DB.ExecuteReader("GetCustomersForPayPoint", CommandSpecies.StoredProcedure);

			foreach (DataRow row in dt.Rows)
				HandleOnePayment(row);
		} // Execute

		#endregion method Execute

		#region property Name

		public override string Name {
			get { return "PayPoint Charger"; }
		} // Name

		#endregion property Name

		#endregion public

		#region private

		#region method HandleOnePayment

		private void HandleOnePayment(DataRow row) {
			var sr = new SafeReader(row);
			int loanScheduleId = sr["id"];
			int loanId = sr["LoanId"];
			string firstName = sr["FirstName"];
			int customerId = sr["CustomerId"];
			string customerMail = sr["Email"];
			string fullname = sr["Fullname"];
			bool reductionFee = sr["ReductionFee"];
			string refNum = sr["RefNum"];
			bool lastInstallment = sr["LastInstallment"];

			decimal amountDue = payPointApi.GetAmountToPay(loanScheduleId);

			if (!ShouldCharge(lastInstallment, amountDue)) {
				Log.Info("Will not charge loan schedule id:{0} (The amount was:{1}). The minimal amount for collection is:{2}", loanScheduleId, amountDue, amountToChargeFrom);
				return;
			} // if

			decimal initialAmountDue = amountDue;

			AutoPaymentResult autoPaymentResult = TryToMakeAutoPayment(loanScheduleId, initialAmountDue, customerId, customerMail,
				fullname, reductionFee);

			if (autoPaymentResult.IsException)
				return;

			if (autoPaymentResult.PaymentFailed) {
				SendFailureMail(firstName, initialAmountDue, customerMail);
				return;
			} // if

			if (autoPaymentResult.PaymentCollectedSuccessfully) {
				SendConfirmationMail(firstName, amountDue, refNum, customerMail);
				SendLoanStatusMail(loanId, firstName, refNum, customerMail);
			} // if
		} // HandleOnePayment

		#endregion method HandleOnePayment

		#region method TryToMakeAutoPayment

		private AutoPaymentResult TryToMakeAutoPayment(int loanScheduleId, decimal initialAmountDue, int customerId, string customerMail, string fullname, bool reductionFee) {
			var result = new AutoPaymentResult();
			decimal actualAmountCharged = initialAmountDue;

			int counter = 0;

			while (counter <= 3) {
				PayPointReturnData payPointReturnData;

				if (MakeAutoPayment(loanScheduleId, actualAmountCharged, out payPointReturnData)) {
					if (IsNotEnoughMoney(payPointReturnData)) {
						if (!reductionFee) {
							result.PaymentFailed = true;
							return result;
						} // if

						counter++;

						if (counter > 3) {
							result.PaymentFailed = true;
							return result;
						} // if

						if (counter == 1)
							actualAmountCharged = Math.Round((initialAmountDue * (decimal)0.75), 2);
						else if (counter == 2)
							actualAmountCharged = Math.Round((initialAmountDue * (decimal)0.5), 2);
						else if (counter == 3)
							actualAmountCharged = Math.Round((initialAmountDue * (decimal)0.25), 2);
					}
					else if (IsCollectionSuccessful(payPointReturnData)) {
						result.PaymentCollectedSuccessfully = true;
						result.ActualAmountCharged = actualAmountCharged;
						return result;
					}
					else {
						result.PaymentFailed = true;
						return result;
					} // if
				}
				else {
					SendExceptionMail(initialAmountDue, customerId, customerMail, fullname);
					result.IsException = true;
					return result;
				} // if
			} // while

			return result;
		} // TryToMakeAutoPayment

		#endregion method TryToMakeAutoPayment

		#region method SendConfirmationMail

		private void SendConfirmationMail(string firstName, decimal amountDue, string refNum, string customerMail) {
			var variables = new Dictionary<string, string> {
				{"AMOUNT", amountDue.ToString(CultureInfo.InvariantCulture)},
				{"FirstName", firstName},
				{"DATE", FormattingUtils.FormatDateToString(DateTime.UtcNow)},
				{"RefNum", refNum}
			};

			mailer.Send("Mandrill - Repayment confirmation", variables, new Addressee(customerMail));
		} // SendConfirmationMail

		#endregion method SendConfirmationMail

		#region method SendLoanStatusMail

		private void SendLoanStatusMail(int loanId, string firstName, string refNum, string customerMail) {
			DataTable dt = DB.ExecuteReader(
				"GetLoanStatus",
				CommandSpecies.StoredProcedure,
				new QueryParameter("LoanId", loanId)
			);

			var sr = new SafeReader(dt.Rows[0]);

			string loanStatus = sr["Status"];

			if (loanStatus == "PaidOff") {
				var variables = new Dictionary<string, string> {
					{"FirstName", firstName},
					{"RefNum", refNum}
				};

				mailer.Send("Mandrill - Loan paid in full", variables, new Addressee(customerMail));
			} // if
		} // SendLoanStatusMail

		#endregion method SendLoanStatusMail

		#region method SendFailureMail

		private void SendFailureMail(string firstName, decimal initialAmountDue, string customerMail) {
			var variables = new Dictionary<string, string> {
				{"FirstName", firstName},
				{"AmountOwed", initialAmountDue.ToString(CultureInfo.InvariantCulture)},
				{"DueDate", FormattingUtils.FormatDateToString(DateTime.UtcNow)}
			};

			mailer.Send("Mandrill - Automatic Re-Payment has Failed", variables, new Addressee(customerMail));
		} // SendFailureMail

		#endregion method SendFailureMail

		#region method SendExceptionMail

		private void SendExceptionMail(decimal initialAmountDue, int customerId, string customerMail, string fullName) {
			mailer.Send("Mandrill - PayPoint Script Exception", new Dictionary<string, string> {
				{"UserID", customerId.ToString(CultureInfo.InvariantCulture)},
				{"Email", customerMail},
				{"FullName", fullName},
				{"Amount", initialAmountDue.ToString(CultureInfo.InvariantCulture)}
			});
		} // SendExceptionMail

		#endregion method SendExceptionMail

		#region method ShouldCharge

		private bool ShouldCharge(bool lastInstallment, decimal amountDue) {
			return (lastInstallment && amountDue > 0) || amountDue >= amountToChargeFrom;
		} // ShouldCharge

		#endregion method ShouldCharge

		#region method MakeAutoPayment

		private bool MakeAutoPayment(int loanScheduleId, decimal amountDue, out PayPointReturnData result) {
			try {
				result = payPointApi.MakeAutomaticPayment(loanScheduleId, amountDue);
				return true;
			}
			catch (Exception ex) {
				Log.Error("Failed making auto payment for loan schedule id:{0} exception:{1}", loanScheduleId, ex);
				result = null;
				return false;
			} // try
		} // MakeAutoPayment

		#endregion method MakeAutoPayment

		#region method IsCollectionSuccessful

		private static bool IsCollectionSuccessful(PayPointReturnData payPointReturnData) {
			return payPointReturnData.Code == "A";
		} // IsCollectionSuccessful

		#endregion method IsCollectionSuccessful

		#region method IsNotEnoughMoney

		private static bool IsNotEnoughMoney(PayPointReturnData payPointReturnData) {
			return payPointReturnData.Code == "P:A";
		} // IsNotEnoughMoney

		#endregion method IsNotEnoughMoney

		private readonly int amountToChargeFrom;
		private readonly StrategiesMailer mailer;
		private readonly PayPointApi payPointApi = new PayPointApi();

		#endregion private
	} // PayPointCharger

	#endregion class PayPointCharger
} // namespace
