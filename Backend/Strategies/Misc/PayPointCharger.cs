namespace Ezbob.Backend.Strategies.Misc {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using DbConstants;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Backend.Strategies.MailStrategies;
	using Ezbob.Backend.Strategies.NewLoan;
	using Ezbob.Backend.Strategies.NewLoan.Exceptions;
	using Ezbob.Database;
	using EZBob.DatabaseLib.Model.Database;
	using PaymentServices.PayPoint;

	// TODO : SP NL_GetNumOfActiveLoans.sql - add to where status write off also -> commented search :  //"NL_GetNumOfActiveLoans

	public class AutoPaymentResult {
		public decimal ActualAmountCharged { get; set; }
		public bool PaymentFailed { get; set; }
		public bool PaymentCollectedSuccessfully { get; set; }
		public bool IsException { get; set; }
	}//class AutoPaymentResult

	public class PayPointCharger : AStrategy {
		private readonly int[] dueChargingDays = { 0, 1, 3, 7, 14, 21, 28 };
		private readonly int amountToChargeFrom;
		private readonly PayPointApi payPointApi = new PayPointApi();

		public PayPointCharger() {
			SafeReader sr = DB.GetFirst("PayPointChargerGetConfigs", CommandSpecies.StoredProcedure);
			this.amountToChargeFrom = sr["AmountToChargeFrom"];
		}//constructor

		private object[] strategyArgs;
		public string Error { get; private set; }

		public override void Execute() {

			// step 1 - Get loans to pay.
			try {
				IEnumerable<SafeReader> loansList = DB.ExecuteEnumerable("GetCustomersForPayPoint", CommandSpecies.StoredProcedure);
				IEnumerable<SafeReader> nlList = DB.ExecuteEnumerable("NL_CustomersForAutoCharger", CommandSpecies.StoredProcedure, new QueryParameter("Now", DateTime.UtcNow)).ToList();

				this.strategyArgs = new object[] { loansList, nlList };

				NL_AddLog(LogType.Info, "Strategy Start", this.strategyArgs, Error, null, null);

				foreach (var loan in loansList) {
					var nlLoan = nlList.FirstOrDefault(ldata => ldata["OldLoanID"] == loan["LoanId"]);
					try {
						HandleOnePayment(loan, nlLoan);
						// ReSharper disable once CatchAllClause
					} catch (Exception ex) {
						Error = string.Format("failed to auto charge customer {0} schedule {1}", loan["CustomerId"], loan["LoanScheduleId"]);
						Log.Error(ex, Error);
						NL_AddLog(LogType.Error, "loan iteration failed", this.strategyArgs, Error, ex.Message, ex.StackTrace);
					}
				}

				Error = string.Empty;
				NL_AddLog(LogType.Info, "Strategy End", this.strategyArgs, Error, null, null);

				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {
				Error = ex.Message;
				Log.Error(Error);
				NL_AddLog(LogType.Error, "Strategy Faild", this.strategyArgs, Error, ex.ToString(), ex.StackTrace);
			}
		}//Execute

		public override string Name { get { return "PayPoint Charger"; } }

		private bool IsRegulated(TypeOfBusiness typeOfBusiness) {
			return new[] {
	            TypeOfBusiness.Limited,
	            TypeOfBusiness.LLP,
	            TypeOfBusiness.PShip
	        }.Contains(typeOfBusiness);
		}


		public decimal GetAmountToPay(int customerID, int loanScheduleID, int loandID) {

			NL_Model nlModel = new NL_Model(customerID) { Loan = new NL_Loans() };
			GetLoanState state = new GetLoanState(customerID, loandID, DateTime.UtcNow);

			try {
				state.Execute();
				nlModel = state.Result;
			} catch (NL_ExceptionInputDataInvalid nlExceptionInputDataInvalid) {
				Error = nlExceptionInputDataInvalid.Message;
				Log.Error(Error);
				NL_AddLog(LogType.Error, "Failed to GetLoanState for NL loan", new object[] { customerID, loandID, loanScheduleID }, Error, nlExceptionInputDataInvalid.ToString(), nlExceptionInputDataInvalid.StackTrace);
			}

			if (!string.IsNullOrEmpty(state.Error)) {
				Error = state.Error;
				Log.Error(Error);
				NL_AddLog(LogType.Error, "Failed to GetLoanState for NL loan", new object[] { customerID, loandID, loanScheduleID }, Error, Error, null);
			}

			var item = nlModel.Loan.LastHistory().Schedule.FirstOrDefault(s => s.LoanScheduleID == loanScheduleID);
			if (item != null) {
				return item.AmountDue;
			}

			Error = string.Format("Failed to get AmountDue for nlloanID {0}, schedule={1}", loandID, loanScheduleID);
			Log.Error(Error);
			NL_AddLog(LogType.Error, "Failed to get AmountDue", new object[] { customerID, loandID, loanScheduleID }, Error, Error, null);

			return 0;
		}

		private void HandleOnePayment(SafeReader loan, SafeReader nlLoan = null) {
			string message;
			int loanScheduleId = loan["LoanScheduleId"];
			int loanId = loan["LoanId"];
			//string firstName = loan["FirstName"];
			int customerId = loan["CustomerId"];
			string customerMail = loan["Email"];
			string fullname = loan["Fullname"];
			string typeOfBusinessStr = loan["TypeOfBusiness"];
			DateTime dueDate = loan["DueDate"]; // PlannedDate
			bool reductionFee = loan["ReductionFee"];
			string refNum = loan["RefNum"]; // of loan
			bool lastInstallment = loan["LastInstallment"]; // bit

			TypeOfBusiness typeOfBusiness = (TypeOfBusiness)Enum.Parse(typeof(TypeOfBusiness), typeOfBusinessStr);
			bool isNonRegulated = IsRegulated(typeOfBusiness);

			DateTime now = DateTime.UtcNow;
			TimeSpan span = now.Subtract(dueDate);
			int daysLate = (int)span.TotalDays;

			// amount due.
			decimal amountDue = this.payPointApi.GetAmountToPay(loanScheduleId);

			NL_Payments nlPayment = null;

			if (nlLoan == null) {
				NL_AddLog(LogType.Info, string.Format("could not find corresponding nl_loan for oldLoanID : {0}", loanId), this.strategyArgs, null, null, null);
			} else {
				// new loan amount due.  
				decimal nlAmountDue = GetAmountToPay(nlLoan["CustomerId"], nlLoan["LoanId"], nlLoan["LoanScheduleId"]);

				message = string.Format("LoanID={0} oldLoanID={1} amountDue= {2} nlAmountDue={3}", nlLoan["LoanId"], nlLoan["OldLoanID"], amountDue, nlAmountDue);
				Log.Debug(message);
				NL_AddLog(LogType.Info, "AmountDue", this.strategyArgs, message, null, null);

				nlPayment = new NL_Payments() {
					PaymentMethodID = (int)NLLoanTransactionMethods.Auto,
					Amount = nlAmountDue,
					PaymentStatusID = (int)NLPaymentStatuses.InProgress,
					PaymentSystemType = NLPaymentSystemTypes.Paypoint,
					CreationTime = now,
					CreatedByUserID = 1,
					Notes = "autocharger",
					LoanID = nlLoan["LoanId"]
				};
			}

			if (!ShouldCharge(lastInstallment, amountDue)) {
				message = string.Format("Will not charge loan schedule id {0} (amount {1}): the minimal amount for collection is {2}.", loanScheduleId, amountDue, this.amountToChargeFrom);
				Log.Info(message);
				if (nlPayment != null)
					message = String.Concat(message, string.Format(" nlPayment={0}", nlPayment));
				NL_AddLog(LogType.Info, "Exit 1", this.strategyArgs, message, null, null);
				return;
			}//if

			decimal initialAmountDue = amountDue;

			if ((!isNonRegulated && daysLate > 3) || !this.dueChargingDays.Contains(daysLate)) {
				message = string.Format("Will not charge loan schedule id {0} (amount {1}): the charging is not scheduled today", loanScheduleId, amountDue);
				Log.Info(message);
				if (nlPayment != null)
					message = String.Concat(message, string.Format(" nlPayment={0}", nlPayment));
				NL_AddLog(LogType.Info, "Exit 2", this.strategyArgs, message, null, null);
				return;
			}

			// step 2 - charging
			AutoPaymentResult autoPaymentResult = TryToMakeAutoPayment(
				loanId,
				loanScheduleId,
				initialAmountDue,
				customerId,
				customerMail,
				fullname,
				reductionFee,
				isNonRegulated,
				nlPayment);

			if (autoPaymentResult.IsException || autoPaymentResult.PaymentFailed) {
				Error = string.Format("Failed collection from customer:{0} amount:{1}, loanID={2}", customerId, initialAmountDue, loanId);
				Log.Warn(Error);
				if (nlPayment != null)
					Error = String.Concat(Error, string.Format(" nlPayment={0}", nlPayment));
				NL_AddLog(LogType.Info, "Exit 3", this.strategyArgs, Error, null, null);
				return;
			} // if

			// step 4 - notifications
			if (autoPaymentResult.PaymentCollectedSuccessfully) {
				// send mail to customer from template "Mandrill - Repayment confirmation"
				PayEarly payEarly = new PayEarly(customerId, autoPaymentResult.ActualAmountCharged, refNum);
				payEarly.Execute();

				SendLoanStatusMail(customerId, loanId, customerMail, autoPaymentResult.ActualAmountCharged); // Will send mail for paid off loans
			}//if
		}//HandleOnePayment

		/// <summary>
		/// 
		/// </summary>
		/// <param name="loanId"></param>
		/// <param name="loanScheduleId"></param>
		/// <param name="initialAmountDue"></param>
		/// <param name="customerId"></param>
		/// <param name="customerMail"></param>
		/// <param name="fullname"></param>
		/// <param name="reductionFee"></param>
		/// <param name="isNonRegulated"></param>
		/// <param name="nlPayment"></param>
		/// <returns></returns>
		private AutoPaymentResult TryToMakeAutoPayment(
			int loanId,
			int loanScheduleId,
			decimal initialAmountDue,
			int customerId,
			string customerMail,
			string fullname,
			bool reductionFee,
			bool isNonRegulated,
			NL_Payments nlPayment = null) {

			var result = new AutoPaymentResult();
			decimal actualAmountCharged = initialAmountDue;
			int counter = 0;

			while (counter <= 2) {

				PayPointReturnData payPointReturnData;

				string message;

				if (MakeAutoPayment(customerId, loanId, loanScheduleId, actualAmountCharged, out payPointReturnData, nlPayment)) {

					if (isNonRegulated && IsNotEnoughMoney(payPointReturnData)) {

						if (!reductionFee) {
							result.PaymentFailed = true;

							message = string.Format("payPointReturnData={0} result={1}", payPointReturnData, result);
							NL_AddLog(LogType.Info, "NonRegulated+IsNotEnoughMoney", new object[] { customerId, loanId, loanScheduleId, initialAmountDue, nlPayment }, message, null, null);

							return result;
						}

						counter++;

						if (counter > 2) {
							message = string.Format("payPointReturnData={0} result={1}", payPointReturnData, result);

							NL_AddLog(LogType.Info, "More that 2 tryings", new object[] { customerId, loanId, loanScheduleId, initialAmountDue, nlPayment }, message, null, null);

							return result;
						}

						if (counter == 1) {
							actualAmountCharged = Math.Round((initialAmountDue * (decimal)0.5), 2);

							message = string.Format("Trying to charge 50% (Attempt #{0}). Customer:{1} Original amount:{2} Calculated amount:{3}. payPointReturnData={4}", counter + 1, customerId, initialAmountDue, actualAmountCharged, payPointReturnData);

							Log.Info(message);

							NL_AddLog(LogType.Info, "Counter==1", new object[] { customerId, loanId, loanScheduleId, initialAmountDue, nlPayment }, message, null, null);

						} else if (counter == 2) {
							actualAmountCharged = Math.Round((initialAmountDue * (decimal)0.25), 2);

							message = string.Format("Trying to charge 25% (Attempt #{0}). Customer:{1} Original amount:{2} Calculated amount:{3}, payPointReturnData={4}", counter + 1, customerId, initialAmountDue, actualAmountCharged, payPointReturnData);

							Log.Info(message);

							NL_AddLog(LogType.Info, "Counter==2", new object[] { customerId, loanId, loanScheduleId, initialAmountDue, nlPayment }, message, null, null);
						}

					} else if (IsCollectionSuccessful(payPointReturnData)) {
						result.PaymentCollectedSuccessfully = true;
						result.ActualAmountCharged = actualAmountCharged;

						message = string.Format("payPointReturnData={0} result={1}", payPointReturnData, result);
						NL_AddLog(LogType.Info, "Collection Successful", new object[] { customerId, loanId, loanScheduleId, initialAmountDue, nlPayment }, message, null, null);

						return result;
					} else {
						result.PaymentFailed = true;
						message = string.Format("payPointReturnData={0} result={1}", payPointReturnData, result);

						NL_AddLog(LogType.Info, "Collection Failed", new object[] { customerId, loanId, loanScheduleId, initialAmountDue, nlPayment }, message, message, null);

						return result;
					}//if
				} else {
					SendExceptionMail(initialAmountDue, customerId, customerMail, fullname);
					result.IsException = true;

					message = string.Format("payPointReturnData={0} result={1}", payPointReturnData, result);
					NL_AddLog(LogType.Info, "Collection failed -send mail", new object[] { customerId, loanId, loanScheduleId, initialAmountDue, nlPayment }, message, null, null);

					return result;
				} //if

			} //while

			return result;
		} //TryToMakeAutoPayment


		private void SendLoanStatusMail(int customerId, int loanId, string customerMail, decimal actualAmountCharged) {
			LoanStatusAfterPayment loanStatusAfterPayment = new LoanStatusAfterPayment(customerId, customerMail, loanId, actualAmountCharged, true);
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
			return (lastInstallment && amountDue > 0) || amountDue >= this.amountToChargeFrom;
		}//ShouldCharge

		private bool MakeAutoPayment(int customerId, int loanId, int loanScheduleId, decimal amountDue, out PayPointReturnData result, NL_Payments nlPayment = null) {
			try {
				result = this.payPointApi.MakeAutomaticPayment(customerId, loanId, loanScheduleId, amountDue, nlPayment);

				NL_AddLog(LogType.Debug, "MakeAutoPayment success", new object[] { customerId, loanId, loanScheduleId, amountDue, nlPayment }, result, null, null);
				return true;

				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {
				Error = string.Format("Failed making auto payment for loan schedule id:{0} exception:{1}", loanScheduleId, ex);
				Log.Error(Error);

				NL_AddLog(LogType.Error, "MakeAutoPayment failed", new object[] { customerId, loanId, loanScheduleId, amountDue, nlPayment }, Error, ex.Message, ex.StackTrace);
				result = null;
				return false;
			}//try
		}//MakeAutoPayment

		private static bool IsCollectionSuccessful(PayPointReturnData payPointReturnData) {
			return payPointReturnData.Code == "A";
		}//IsCollectionSuccessful

		private bool IsNotEnoughMoney(PayPointReturnData payPointReturnData) {
			bool isNotEnoughMoney = payPointReturnData.Code == "N" && payPointReturnData.Error == "INSUFF FUNDS";
			Error = string.Format("Checking if return status is INSUFF FUNDS. Code:{0} Message:{1} AuthCode:{2} RespCode:{3} Error:{4} Result:{5}",
				payPointReturnData.Code, payPointReturnData.Message, payPointReturnData.AuthCode, payPointReturnData.RespCode, payPointReturnData.Error, isNotEnoughMoney);
			Log.Debug(Error);

			NL_AddLog(LogType.Info, "IsNotEnoughMoney", this.strategyArgs, Error, null, null);

			return isNotEnoughMoney;
		}//IsNotEnoughMoney
	}//class
}//ns
