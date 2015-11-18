namespace Ezbob.Backend.Strategies.Misc {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using DbConstants;
	using Ezbob.Backend.CalculateLoan.LoanCalculator;
	using Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Backend.Strategies.MailStrategies;
	using Ezbob.Backend.Strategies.NewLoan;
	using Ezbob.Database;
	using EZBob.DatabaseLib.Model.Database;
	using PaymentServices.PayPoint;

	// TODO : SP NL_GetNumOfActiveLoans.sql - add to where status write off also

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
			amountToChargeFrom = sr["AmountToChargeFrom"];
		}//constructor

		public override void Execute() {
			// step 1 - get NL data for paypoint iteration
		/*	DB.ForEachRowSafe(
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
			);*/

			//IEnumerable<SafeReader> oldList  = DB.ExecuteEnumerable("GetCustomersForPayPoint", CommandSpecies.StoredProcedure);

			//IEnumerable<SafeReader> newList  = DB.ExecuteEnumerable("NLGetCustomersForPayPoint", CommandSpecies.StoredProcedure);

			//foreach (var sr in oldList) {

			//	var nlloan = newList.FirstOrDefault(x => x["OldLoanID"] == sr["LoanId"]);

			//	HandleOnePayment(sr, nlloan);
			//}

			//el: TODO: call SP NL_LoadLoanForAutomaticPayment - to get all loans to pay today
		}//Execute


		public override string Name { get { return "PayPoint Charger"; } }


		private bool IsRegulated(TypeOfBusiness typeOfBusiness) {
			return new[] {
	            TypeOfBusiness.Limited,
	            TypeOfBusiness.LLP,
	            TypeOfBusiness.PShip
	        }.Contains(typeOfBusiness);
		}

		/// <exception cref="OverflowException">The result is outside the range of a <see cref="T:System.Decimal" />.</exception>
		/// <exception cref="InvalidInitialAmountException">Condition. </exception>
		/// <exception cref="NoLoanHistoryException">Condition. </exception>
		/// <exception cref="InvalidInitialInterestRateException">Condition. </exception>
		/// <exception cref="NoInitialDataException">Condition. </exception>
		/// <exception cref="LoanPendingStatusException">Condition. </exception>
		/// <exception cref="LoanWriteOffStatusException">Condition. </exception>
		/// <exception cref="LoanPaidOffStatusException">Condition. </exception>
		/// <exception cref="Exception">A delegate callback throws an exception. </exception>
		/// <exception cref="NoInstallmentFoundException">Condition. </exception>
		/// <exception cref="InvalidCastException"><paramref /> cannot be cast to the element type of the current <see cref="T:System.Array" />.</exception>
		/// <exception cref="NoScheduleException">Condition. </exception>
		/// <exception cref="NoLoanEventsException">Condition. </exception>
		public decimal GetAmountToPay(int customerID,
									  int loanScheduleID,
									  int loandID) {

			NL_Model nlModel = new NL_Model(customerID) { Loan = new NL_Loans() };
			GetLoanDBState strategy = new GetLoanDBState(nlModel, loandID, DateTime.UtcNow);
			strategy.Execute();
			nlModel = strategy.Result;
			// TODO wrap with try/catch
			ALoanCalculator calc = new LegacyLoanCalculator(nlModel);
			calc.GetState();
			// ### wrap with try/catch
			var nlLoanSchedules = nlModel.Loan.LastHistory().Schedule.FirstOrDefault(x => x.LoanScheduleID == loanScheduleID);
			if (nlLoanSchedules != null) {
				return nlLoanSchedules.AmountDue;
			}
			return 0;
		}


		private void HandleOnePayment(SafeReader sr) {
			int loanScheduleId = sr["LoanScheduleId"];
			int loanId = sr["LoanId"];
			string firstName = sr["FirstName"];
			int customerId = sr["CustomerId"];
			string customerMail = sr["Email"];
			string fullname = sr["Fullname"];
			string typeOfBusinessStr = sr["TypeOfBusiness"];
			DateTime dueDate = sr["DueDate"]; // PlannedDate
			bool reductionFee = sr["ReductionFee"];
			string refNum = sr["RefNum"]; // of loan
			bool lastInstallment = sr["LastInstallment"]; // bit

			TypeOfBusiness typeOfBusiness = (TypeOfBusiness)Enum.Parse(typeof(TypeOfBusiness), typeOfBusinessStr);
			bool isNonRegulated = IsRegulated(typeOfBusiness);

			DateTime now = DateTime.UtcNow;
			TimeSpan span = now.Subtract(dueDate);
			int daysLate = (int)span.TotalDays;

			//Old amount due.
			decimal oldAmountDue = payPointApi.GetAmountToPay(loanScheduleId);

			//New amount due.    
			decimal newAmountDue = GetAmountToPay(customerId, loanId, loanScheduleId);

			NL_AddLog(LogType.Info, Name + " - AmountDue OLD vs NEW", oldAmountDue, newAmountDue, null, null);

			//el: TODO: get NL amount due for relevant schedule item

			if (!ShouldCharge(lastInstallment, oldAmountDue)) {
				Log.Info("Will not charge loan schedule id {0} (amount {1}): the minimal amount for collection is {2}.",
						 loanScheduleId, oldAmountDue, amountToChargeFrom);
				return;
			}//if

			decimal initialAmountDue = oldAmountDue;

			if ((!isNonRegulated && daysLate > 3) || !this.dueChargingDays.Contains(daysLate)) {
				Log.Info("Will not charge loan schedule id {0} (amount {1}): the charging is not scheduled today",
						 loanScheduleId, oldAmountDue);
				return;
			}

			// TODO - remove
			var newLoanId = DB.ExecuteScalar<long>("GetNewLoanIdByOldLoanId", CommandSpecies.StoredProcedure, new QueryParameter("@LoanID", loanId));

			// step 2 - charging
			AutoPaymentResult autoPaymentResult = TryToMakeAutoPayment(
				loanId,
				newLoanId, // TODO - move to the end of arguments list
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

			// step 4 - notifications
			if (autoPaymentResult.PaymentCollectedSuccessfully) {

				PayEarly payEarly = new PayEarly(customerId, autoPaymentResult.ActualAmountCharged, refNum);
				payEarly.Execute();

				SendLoanStatusMail(customerId, loanId, customerMail, autoPaymentResult.ActualAmountCharged); // Will send mail for paid off loans
			}//if
		}//HandleOnePayment

		private AutoPaymentResult TryToMakeAutoPayment(int loanId,
			long newLoanId, // TODO rename to nlLoanID 
			int loanScheduleId, decimal initialAmountDue, int customerId, string customerMail, string fullname, bool reductionFee, bool isNonRegulated) {
			var result = new AutoPaymentResult();
			decimal actualAmountCharged = initialAmountDue;

			NL_Payments nlp = new NL_Payments() {
				Amount = actualAmountCharged,
				LoanID = newLoanId,
				CreatedByUserID = 0,
				CreationTime = DateTime.UtcNow,
				PaymentMethodID = (int)NLLoanTransactionMethods.Auto,
				PaypointTransactions = new List<NL_PaypointTransactions>()
			};

			int counter = 0;

			while (counter <= 2) {
				PayPointReturnData payPointReturnData;

				if (MakeAutoPayment(loanScheduleId, actualAmountCharged, out payPointReturnData, ref nlp)) {
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

			/*	var nlStrategy = new AddPayment(nlp);
				nlStrategy.Execute();*/

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
			return (lastInstallment && amountDue > 0) || amountDue >= amountToChargeFrom;
		}//ShouldCharge

		private bool MakeAutoPayment(int loanScheduleId, decimal amountDue, out PayPointReturnData result,
			ref NL_Payments nlp // TODO rename to nlPayment
			) {
			try {
				result = payPointApi.MakeAutomaticPayment(loanScheduleId, amountDue, ref nlp);
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
