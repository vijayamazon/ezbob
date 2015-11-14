namespace Ezbob.Backend.Strategies.Misc {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using Ezbob.Backend.Strategies.MailStrategies;
	using Ezbob.Database;
	using PaymentServices.PayPoint;

	public class AutoPaymentResult {
		public decimal ActualAmountCharged { get; set; }
		public bool PaymentFailed { get; set; }
		public bool PaymentCollectedSuccessfully { get; set; }
		public bool IsException { get; set; }
	}//class AutoPaymentResult

	public class PayPointCharger : AStrategy {
		private readonly int[] dueChargingDays = {0, 1, 3, 7, 14, 21, 28 };
		private readonly int amountToChargeFrom;
		private readonly PayPointApi payPointApi = new PayPointApi();

		public PayPointCharger() {
			SafeReader sr = DB.GetFirst("PayPointChargerGetConfigs", CommandSpecies.StoredProcedure);
			amountToChargeFrom = sr["AmountToChargeFrom"];
		}//constructor

		public override void Execute() {
			DB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					try {
						HandleOnePayment(sr);
					} catch (Exception ex) {
						Log.Error(ex, "failed to auto charge customer {0} schedule {1}", sr["CustomerId"], sr["LoanScheduleId"]);
					}
					return ActionResult.Continue;
				},
				"GetCustomersForPayPoint",
				CommandSpecies.StoredProcedure
			);


			//el: TODO: call SP NL_LoadLoanForAutomaticPayment - to get all loans to pay today
		}//Execute


		public override string Name { get { return "PayPoint Charger"; } }

		private void HandleOnePayment(SafeReader sr) {
			int loanScheduleId = sr["LoanScheduleId"];
			int loanId = sr["LoanId"];
			string firstName = sr["FirstName"];
			int customerId = sr["CustomerId"];
			string customerMail = sr["Email"];
			string fullname = sr["Fullname"];
			string typeOfBusiness = sr["TypeOfBusiness"];
			DateTime dueDate = sr["DueDate"]; // PlannedDate
			bool reductionFee = sr["ReductionFee"];
			string refNum = sr["RefNum"]; // of loan
			bool lastInstallment = sr["LastInstallment"]; // bit

			bool isNonRegulated = typeOfBusiness == "Limited" || typeOfBusiness == "LLP" || typeOfBusiness == "PShip";

			DateTime now = DateTime.UtcNow;

			TimeSpan span = now.Subtract(dueDate);
			
			int daysLate = (int)span.TotalDays;

			decimal amountDue = payPointApi.GetAmountToPay(loanScheduleId);

			/*var payEarlyCalc = new LoanRepaymentScheduleCalculator(loan, dateTime, this.amountToChargeFrom);
			return payEarlyCalc.GetState();
			 * return state.AmountDue;
			 * */

			//el: TODO: call SP NL_LoadLoanForAutomaticPayment - to get all loans to pay today

			if (!ShouldCharge(lastInstallment, amountDue)) {
				Log.Info("Will not charge loan schedule id {0} (amount {1}): the minimal amount for collection is {2}.",
						 loanScheduleId, amountDue, amountToChargeFrom);
				return;
			}//if

			decimal initialAmountDue = amountDue;

			if ((!isNonRegulated && daysLate > 3) || !this.dueChargingDays.Contains(daysLate)) {
				Log.Info("Will not charge loan schedule id {0} (amount {1}): the charging is not scheduled today",
						 loanScheduleId, amountDue);
				return;
			}

			AutoPaymentResult autoPaymentResult = TryToMakeAutoPayment(
				loanScheduleId,
				initialAmountDue,
				customerId,
				customerMail,
				fullname,
				reductionFee,
				isNonRegulated
				);

			if (autoPaymentResult.IsException || autoPaymentResult.PaymentFailed) {
				Log.Warn("Failed collection from customer:{0} amount:{1}", customerId, initialAmountDue);
				return;
			} // if

			if (autoPaymentResult.PaymentCollectedSuccessfully) {
				SendConfirmationMail(customerId, autoPaymentResult.ActualAmountCharged, refNum);
				SendLoanStatusMail(customerId, loanId, customerMail, autoPaymentResult.ActualAmountCharged); // Will send mail for paid off loans
			}//if
		}//HandleOnePayment

		private AutoPaymentResult TryToMakeAutoPayment(int loanScheduleId, decimal initialAmountDue, int customerId, string customerMail, string fullname, bool reductionFee, bool isNonRegulated) {
			var result = new AutoPaymentResult();
			decimal actualAmountCharged = initialAmountDue;

			int counter = 0;

			while (counter <= 2) {
				PayPointReturnData payPointReturnData;

				if (MakeAutoPayment(loanScheduleId, actualAmountCharged, out payPointReturnData)) {
					if (isNonRegulated && IsNotEnoughMoney(payPointReturnData)) {
						if (!reductionFee) {
							result.PaymentFailed = true;
							return result;
						}

						counter++;

						if (counter > 2) {
							result.PaymentFailed = true;
							return result;
						}

						if (counter == 1) {
							actualAmountCharged = Math.Round((initialAmountDue * (decimal)0.5), 2);
							Log.Info("Trying to charge 50% (Attempt #{0}). Customer:{1} Original amount:{2} Calculated amount:{3}", counter + 1, customerId, initialAmountDue, actualAmountCharged);
						} else if (counter == 2) {
							actualAmountCharged = Math.Round((initialAmountDue * (decimal)0.25), 2);
							Log.Info("Trying to charge 25% (Attempt #{0}). Customer:{1} Original amount:{2} Calculated amount:{3}", counter + 1, customerId, initialAmountDue, actualAmountCharged);
						}

					} else if (IsCollectionSuccessful(payPointReturnData)) {
						result.PaymentCollectedSuccessfully = true;
						result.ActualAmountCharged = actualAmountCharged;
						return result;
					} else {
						result.PaymentFailed = true;
						return result;
					}//if
				} else {
					SendExceptionMail(initialAmountDue, customerId, customerMail, fullname);
					result.IsException = true;
					return result;
				} //if
			} //while

			return result;
		} //TryToMakeAutoPayment

		private void SendConfirmationMail(int customerId, decimal amountDue, string refNum) {
			PayEarly payEarly = new PayEarly(customerId, amountDue, refNum);
			payEarly.Execute();
		} //SendConfirmationMail

		private void SendLoanStatusMail(int customerId, int loanId, string customerMail, decimal actualAmountCharged) {
			SafeReader sr = DB.GetFirst(
				"GetLoanStatus",
				CommandSpecies.StoredProcedure,
				new QueryParameter("LoanId", loanId)
			);

			string loanStatus = sr["Status"];
			decimal balance = sr["Balance"];

			LoanStatusAfterPayment loanStatusAfterPayment = new LoanStatusAfterPayment(customerId, customerMail, loanId, actualAmountCharged, balance, loanStatus == "PaidOff", true);
			loanStatusAfterPayment.Execute();
		}//SendLoanStatusMail

		private void SendExceptionMail(decimal initialAmountDue, int customerId, string customerMail, string fullName) {

			var variables = new Dictionary<string, string> {
				{"UserID", customerId.ToString(CultureInfo.InvariantCulture)},
				{"Email", customerMail},
				{"FullName", fullName},
				{"Amount", initialAmountDue.ToString(CultureInfo.InvariantCulture)}
			};

			PayPointChargerMails payPointChargerMails = new PayPointChargerMails(customerId, "Mandrill - PayPoint Script Exception", variables);
			payPointChargerMails.Execute();
		}//SendExceptionMail

		private bool ShouldCharge(bool lastInstallment, decimal amountDue) {
			return (lastInstallment && amountDue > 0) || amountDue >= amountToChargeFrom;
		}//ShouldCharge

		private bool MakeAutoPayment(int loanScheduleId, decimal amountDue, out PayPointReturnData result) {
			try {
				result = payPointApi.MakeAutomaticPayment(loanScheduleId, amountDue);
				return true;
			} catch (Exception ex) {
				Log.Error("Failed making auto payment for loan schedule id:{0} exception:{1}", loanScheduleId, ex);
				result = null;
				return false;
			}//try
		}//MakeAutoPayment

		private static bool IsCollectionSuccessful(PayPointReturnData payPointReturnData) {
			return payPointReturnData.Code == "A";
		}//IsCollectionSuccessful

		private bool IsNotEnoughMoney(PayPointReturnData payPointReturnData) {
			bool isNotEnoughMoney = payPointReturnData.Code == "N" && payPointReturnData.Error == "INSUFF FUNDS";
			Log.Debug("Checking if return status is INSUFF FUNDS. Code:{0} Message:{1} AuthCode:{2} RespCode:{3} Error:{4} Result:{5}",
				payPointReturnData.Code, payPointReturnData.Message, payPointReturnData.AuthCode, payPointReturnData.RespCode, payPointReturnData.Error, isNotEnoughMoney);
			return isNotEnoughMoney;
		}//IsNotEnoughMoney
	}//class
}//ns
