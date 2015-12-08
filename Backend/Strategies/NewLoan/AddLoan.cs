namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Backend.CalculateLoan.LoanCalculator;
	using Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Backend.Strategies.NewLoan.Exceptions;
	using Ezbob.Database;
	using MailApi;

	public class AddLoan : AStrategy {

		/// <summary>
		/// Create new loan
		/// Expected input:
		///		NL_Model with:
		///			- CustomerID
		///			- Loan.OldLoanID
		///			- Loan.Refnum
		///			- Histories => history: EventTime (former IssuedTime)
		///			- Agreements
		///			- AgreementModel (JSON)
		///		Expected result:
		///			- int LoanID newly created
		///			- optional string Error
		/// 
		/// Creation of loan from underwriter not supported
		/// mail notifications sent on success/on error
		/// </summary>
		/// <param name="nlModel"></param>
		public AddLoan(NL_Model nlModel) {
			model = nlModel;
			this.strategyArgs = new object[] {model};
		}

		public override string Name { get { return "AddLoan"; } }
		public string Error { get; private set; }
		public long LoanID;
		public NL_Model model { get; private set; }

		private readonly object[] strategyArgs;

		/**
			- loan
			- fees
			- history
			- schedules
			- broker comissions ? - update NLLoanID
			- fund transfer
			- pacnet transaction
			- agreements
			- loan options row
			*/
		public override void Execute() {

			NL_AddLog(LogType.Info, "Strategy Start", this.strategyArgs, Error, null, null);

			try {
				if (model.CustomerID == 0) {
					Error = NL_ExceptionCustomerNotFound.DefaultMessage;
                    NL_AddLog(LogType.Error, "Strategy Faild", this.strategyArgs, null, Error, null);
					return;
				}

				if (model.Loan == null) {
					Error = NL_ExceptionRequiredDataNotFound.Loan;
					NL_AddLog(LogType.Error, "Strategy Faild", this.strategyArgs, null, Error, null);
					return;
				}

				if (model.Loan.OldLoanID == null) {
					Error = NL_ExceptionRequiredDataNotFound.OldLoan;
					NL_AddLog(LogType.Error, "Strategy Faild ", this.strategyArgs, null, Error, null);
					return;
				}

				var history = model.Loan.LastHistory();

				if (history == null) {
					Error = NL_ExceptionRequiredDataNotFound.LastHistory;
                    NL_AddLog(LogType.Error, "Strategy Faild", this.strategyArgs, null, Error, null);
					return;
				}

				if (history.Agreements == null || history.Agreements.Count == 0) {
					Error = string.Format("Expected input data not found (NL_Model initialized by: NLAgreementItem list). Customer {0}", model.CustomerID);
					NL_AddLog(LogType.Error, "Strategy Faild - Failed to generate Schedule/fees", this.strategyArgs, null, Error, null);
                    return;
				}

				if (history.AgreementModel == null) {
					Error = string.Format("Expected input data not found (NL_Model initialized by: AgreementModel in JSON). Customer {0}", model.CustomerID);
					NL_AddLog(LogType.Error, "Strategy Faild - Failed to generate Schedule/fees", this.strategyArgs, null, Error, null);
					return;
				}

				// RepaymentIntervalType, EventTime 
				model.Loan.LastHistory().SetDefaults();
				// LoanType, LoanFormulaID, RepaymentDate
				model.Loan.SetDefaults();

				BuildLoanFromOffer dataForLoan = new BuildLoanFromOffer(model);
				try {
					dataForLoan.Execute();
					// ReSharper disable once CatchAllClause
				} catch (Exception ex) {
					Log.Alert(ex.Message);
				}

				if (!string.IsNullOrEmpty(dataForLoan.Error)) {
					Error = dataForLoan.Error;
					NL_AddLog(LogType.Error, "Strategy Faild - Failed to generate Schedule/fees", this.strategyArgs, null, Error, null);
					return;
				}

				model = dataForLoan.Result;

                // prevent to create the same loan (by refnum)
                if (!string.IsNullOrEmpty(model.Loan.Refnum) && !string.IsNullOrEmpty(dataForLoan.DataForLoan.ExistsRefnums) && dataForLoan.DataForLoan.ExistsRefnums.Contains(model.Loan.Refnum)) {
                    Error = NL_ExceptionLoanExists.DefaultMessage;
                }

				ALoanCalculator nlCalculator = null;
				// 2. Init Schedule and Fees
				try {
					nlCalculator = new LegacyLoanCalculator(model);
					// model should contain Schedule and Fees after this invocation
					nlCalculator.CreateSchedule(); // create primary dates/p/r/f distribution of schedules (P/n) and setup/servicing fees. 7 September - fully completed schedule + fee + amounts due, without payments.
				} catch (NoInitialDataException noDataException) {
					Error = noDataException.Message;
				} catch (InvalidInitialAmountException amountException) {
					Error = amountException.Message;
				} catch (InvalidInitialInterestRateException interestRateException) {
					Error = interestRateException.Message;
				} catch (InvalidInitialRepaymentCountException paymentsException) {
					Error = paymentsException.Message;
					// ReSharper disable once CatchAllClause
				} catch (Exception ex) {
					Error = string.Format("Failed to get calculator instance/Schedule. customer {0}, err: {1}", model.CustomerID, ex.Message);
				} finally {
					if (nlCalculator == null) {
						Error = string.Format("Failed to get calculator instance. customer {0}, err: {1}", model.CustomerID, Error);
					}
				}

				if (!string.IsNullOrEmpty(Error)) {
					Log.Alert("Failed to calculate Schedule. customer {0}, err: {1}", model.CustomerID, Error);
                    NL_AddLog(LogType.Error,
                        "Strategy Faild" + string.Format("Failed to calculate Schedule. customer {0}, err: {1}", model.CustomerID, Error),
						 this.strategyArgs, null, Error, null);
					return;
				}

				List<NL_LoanSchedules> nlSchedule = new List<NL_LoanSchedules>();
				List<NL_LoanFees> nlFees = new List<NL_LoanFees>();
				List<NL_LoanAgreements> nlAgreements = new List<NL_LoanAgreements>();
				DateTime nowTime = DateTime.UtcNow;
				NL_Payments setupfeeOffsetpayment = null;
				NL_LoanFeePayments feePayment = null;

				// get updated history filled with Schedule
				history = model.Loan.LastHistory();

				// copy to local schedules list
				history.Schedule.ForEach(s => nlSchedule.Add(s));

				if (nlSchedule.Count == 0) {
					Error += "Failed to generate Schedule/fees";
					NL_AddLog(LogType.Error, "Strategy Faild - Failed to generate Schedule/fees", this.strategyArgs, null, Error, null);
					return;
				}

				// 3. complete NL_Loans object data
				model.Loan = dataForLoan.Result.Loan;
				model.Loan.CreationTime = nowTime;
				model.Loan.LoanStatusID = (int)NLLoanStatuses.Live;
				model.Loan.Position += 1;

				// copy to local fees list
				model.Loan.Fees.ForEach(f => nlFees.Add(f));

				foreach (NL_LoanFees f in nlFees) {
					f.CreatedTime = nowTime; // from calc-r
					f.AssignedByUserID = 1; //  from calc-r

					// setup fee inserted in NL_LoanFees table; to prevent charging of the fee, NL_Payment inserted with type SetupFeeOffset
					if (f.LoanFeeTypeID == (int)NLFeeTypes.SetupFee) {
						setupfeeOffsetpayment = new NL_Payments {
							PaymentMethodID = (int)NLLoanTransactionMethods.SetupFeeOffset,
							Amount = f.Amount,
							CreatedByUserID = 1,
							CreationTime = f.CreatedTime,
							Notes = "setup fee offsetting",
							PaymentTime = nowTime,
							PaymentStatusID = (int)NLPaymentStatuses.Active
						};
						model.Loan.Payments.Add(setupfeeOffsetpayment);

						//Log.Debug("Created setup offset payment: {0}", setupfeeOffsetpayment);
					}
				}

				ConnectionWrapper pconn = DB.GetPersistent();

				try {

					pconn.BeginTransaction();

					// 4. save loan
					this.LoanID = DB.ExecuteScalar<long>(pconn, "NL_LoansSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", model.Loan));
					model.Loan.LoanID = this.LoanID;

					//Log.Debug("NL_LoansSave: LoanID: {0}", this.LoanID);

					// 5. fees
					nlFees.ForEach(f => f.LoanID = this.LoanID);

					//nlFees.ForEach(f => Log.Debug("Adding fees: {0}", f));

					// insert fees
					DB.ExecuteNonQuery(pconn, "NL_LoanFeesSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanFees>("Tbl", nlFees));

					// 6. broker commissions
					// done in controller. When old loan removed: check if this is the broker's customer, calc broker fees, insert into LoanBrokerCommission
					if (model.Offer.BrokerSetupFeePercent > 0) {
						DB.ExecuteNonQuery(string.Format("UPDATE dbo.LoanBrokerCommission SET NLLoanID = {0} WHERE LoanID = {1}", this.LoanID, model.Loan.OldLoanID));
					}

					// 7. history
					history.LoanID = this.LoanID;
					history.Description = "adding loan. oldID: " + model.Loan.OldLoanID;

					//Log.Debug("Adding history: {0}", history);

					history.LoanHistoryID = DB.ExecuteScalar<long>(pconn, "NL_LoanHistorySave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", history));

					//Log.Debug("NL_LoanHistorySave: LoanID: {0}, LoanHistoryID: {1}", model.Loan.LoanID, history.LoanHistoryID);

					// 8. loan agreements
					history.Agreements.ForEach(a => nlAgreements.Add(a));
					nlAgreements.ForEach(a => a.LoanHistoryID = history.LoanHistoryID);

					//nlAgreements.ForEach(a => Log.Debug("Adding agreement: {0}", a));

					DB.ExecuteNonQuery(pconn, "NL_LoanAgreementsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanAgreements>("Tbl", nlAgreements));

					// 9. schedules 
					nlSchedule.ForEach(s => s.LoanHistoryID = history.LoanHistoryID);

					//nlSchedule.ForEach(s => Log.Debug("Adding schedule: {0}", s));

					DB.ExecuteNonQuery(pconn, "NL_LoanSchedulesSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanSchedules>("Tbl", nlSchedule));

					// 10. Fund Transfer 
					if (model.FundTransfer != null) {

						model.FundTransfer.LoanID = this.LoanID;
						model.FundTransfer.FundTransferID = DB.ExecuteScalar<long>(pconn, "NL_FundTransfersSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", model.FundTransfer));

						//Log.Debug("NL_FundTransfersSave: LoanID: {0}, fundTransferID: {1}", this.LoanID, model.FundTransfer.FundTransferID);
					}

					// 11. save default loan options record
                    model.Loan.LoanOptions.LoanOptionsID = DB.ExecuteScalar<long>(pconn, "NL_SaveLoanOptions",
					   CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", new NL_LoanOptions {
						   LoanID = this.LoanID, 
						   UserID = 1, // default system user?
						   InsertDate = nowTime,
						   IsActive = true,
						   Notes = "default options"
					   }), new QueryParameter("@LoanID", this.LoanID)
					 );

					pconn.Commit();

					// ReSharper disable once CatchAllClause
				} catch (Exception ex) {

					pconn.Rollback();

					this.LoanID = 0;
					Error = ex.Message;
					Log.Error("Failed to add new loan: {0}", Error);

					SendMail("NL: loan rolled back", history, nlFees, nlSchedule, nlAgreements, setupfeeOffsetpayment, feePayment);

                    NL_AddLog(LogType.Error, "Strategy Faild - Failed to add new loan", this.strategyArgs, Error, ex.ToString(), ex.StackTrace);
					return;
				}


				// 7. Pacnet transaction
				try {
					if (model.FundTransfer != null && (model.FundTransfer.PacnetTransactions.Count > 0 && model.FundTransfer.FundTransferID > 0)) {

						var pacnetTransaction = model.FundTransfer.LastPacnetTransactions();
						pacnetTransaction.FundTransferID = model.FundTransfer.FundTransferID;
						pacnetTransaction.PacnetTransactionID = DB.ExecuteScalar<long>("NL_PacnetTransactionsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", pacnetTransaction));

						//Log.Debug("NL_PacnetTransactionsSave: LoanID: {0}, pacnetTransactionID: {1}", this.LoanID, pacnetTransaction.PacnetTransactionID);
					}
					// ReSharper disable once CatchAllClause
				} catch (Exception e1) {

					Error = e1.Message;
					Log.Error("Failed to save PacnetTransaction: {0}", Error);

					// PacnetTransaction error
					SendMail("NL: Failed to save PacnetTransaction", history, nlFees, nlSchedule, nlAgreements, setupfeeOffsetpayment, feePayment);
				}

				// 11. if setup fee - add payment to offset it
				if (setupfeeOffsetpayment != null && this.LoanID > 0) {

					setupfeeOffsetpayment.LoanID = this.LoanID;
					setupfeeOffsetpayment.PaymentID = DB.ExecuteScalar<long>("NL_PaymentsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", setupfeeOffsetpayment));

					//Log.Debug("Added setup offset payment: {0}", setupfeeOffsetpayment);

					var loanFees = DB.Fill<NL_LoanFees>("NL_LoansFeesGet", CommandSpecies.StoredProcedure, new QueryParameter("@LoanID", this.LoanID));

					if (loanFees != null) {
						// must be the only of this type
						var setupFee = loanFees.FirstOrDefault(f => f.LoanFeeTypeID == (int)NLFeeTypes.SetupFee);

						//	register NL_LoanFeePayments record
						if (setupFee != null) {

							feePayment = new NL_LoanFeePayments {
								Amount = setupfeeOffsetpayment.Amount,
								PaymentID = setupfeeOffsetpayment.PaymentID,
								LoanFeeID = setupFee.LoanFeeID
							};

							feePayment.LoanFeePaymentID = DB.ExecuteScalar<long>("NL_LoanFeePaymentsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", feePayment));

							//Log.Debug("Added NL_LoanFeePayments for setup offset payment: {0}", feePayment);
						}
					}
				}

				// OK
				SendMail("NL: Saved successfully", history, nlFees, nlSchedule, nlAgreements, setupfeeOffsetpayment, feePayment);
				NL_AddLog(LogType.Info, "Strategy End", this.strategyArgs, this.LoanID, Error, null);
			} catch (Exception ex) {
				NL_AddLog(LogType.Error, "Strategy Faild", this.strategyArgs, Error, ex.ToString(), ex.StackTrace);
			}
		}//Execute


		private void SendMail(string subject, NL_LoanHistory history, List<NL_LoanFees> fees,
			List<NL_LoanSchedules> schedule, List<NL_LoanAgreements> agreements,
			NL_Payments setupfeeOffsetpayment = null, NL_LoanFeePayments feePayment = null) {

			string emailToAddress = CurrentValues.Instance.Environment.Value.Contains("Dev") ? "elinar@ezbob.com" : CurrentValues.Instance.EzbobTechMailTo;
			string emailFromName = CurrentValues.Instance.MailSenderName;
			string emailFromAddress = CurrentValues.Instance.MailSenderEmail;

			string sMsg = string.Format("{0}. cust {1} user {2}, oldloan {3}, LoanID {4} error: {5}", subject, model.CustomerID, model.UserID, model.Loan.OldLoanID, this.LoanID, Error);

			history.Schedule.Clear();
			history.Schedule = schedule;
			history.Agreements.Clear();
			history.Agreements = agreements;

			model.Loan.Histories.Clear();
			model.Loan.Histories.Add(history);
			model.Loan.Fees.Clear();
			model.Loan.Fees = fees;

			var message = string.Format(
					"<h5>{0}</h5>"
					+ "<h5>Loan</h5> <pre>{1}</pre>"
					+ "<h5>FundTransfer</h5> <pre>{2}</pre>"
					+ "<h5>Setup fee offset payment</h5> <pre>{3}</pre>"
					+ "<h5>Setup fee offset loan-payment</h5> <pre>{4}</pre>",

				HttpUtility.HtmlEncode(sMsg)
				, HttpUtility.HtmlEncode(model.Loan.ToString())
				, HttpUtility.HtmlEncode(model.FundTransfer == null ? "no FundTransfer specified" : model.FundTransfer.ToString())
				, HttpUtility.HtmlEncode(setupfeeOffsetpayment == null ? "" : setupfeeOffsetpayment.ToString())
				, HttpUtility.HtmlEncode(feePayment == null ? "" : feePayment.ToString()));

			new Mail().Send(
				emailToAddress,
				null,				// message text
				message,			//html
				emailFromAddress,	// fromEmail
				emailFromName,		// fromName
				subject
				);

		} // SendMail



	}//class AddLoan
}//ns