namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Web;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Backend.CalculateLoan.LoanCalculator;
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
		}//constructor

		public override string Name { get { return "AddLoan"; } }
		public string Error;
		public long LoanID;
		public NL_Model model { get; private set; }

		/**
			- loan
			- fees
			- history
			- schedules
			- broker comissions ? - update NLLoanID
			- fund transfer
			- pacnet transaction
			- agreements
			*/
		public override void Execute() {

			// TODO check if "credit available" is enough for this loan amount

			if (model.CustomerID == 0) {
				this.Error = NL_ExceptionCustomerNotFound.DefaultMessage;
				return;
			}

			if (model.Loan == null) {
				this.Error = NL_ExceptionRequiredDataNotFound.Loan;
				return;
			}

			if (model.Loan.OldLoanID == null) {
				this.Error = NL_ExceptionRequiredDataNotFound.OldLoan;
				return;
			}

			if (model.Loan.Refnum == null) {
				this.Error = NL_ExceptionRequiredDataNotFound.LoanRefNum;
				return;
			}

			var history = model.Loan.LastHistory();

			if (history == null) {
				this.Error = NL_ExceptionRequiredDataNotFound.LastHistory;
				return;
			}

			if (history.Agreements == null || history.Agreements.Count == 0) {
				this.Error = string.Format("Expected input data not found (NL_Model initialized by: NLAgreementItem list). Customer {0}", model.CustomerID);
				return;
			}

			if (history.AgreementModel == null) {
				this.Error = string.Format("Expected input data not found (NL_Model initialized by: AgreementModel in JSON). Customer {0}", model.CustomerID);
				return;
			}

			// RepaymentCount, EventTime, InterestRate
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
				this.Error = dataForLoan.Error;
				return;
			}

			model = dataForLoan.Result;

			//ALoanCalculator nlCalculator = new LegacyLoanCalculator(model);
			//if (model.CalculatorImplementation.GetType() == typeof(BankLikeLoanCalculator))
			//	nlCalculator = new BankLikeLoanCalculator(model);

		    ALoanCalculator nlCalculator = null;
			// 2. Init Schedule and Fees
			try {
                nlCalculator = new LegacyLoanCalculator(model);
				// model should contain Schedule and Fees after this invocation
				nlCalculator.CreateSchedule(); // create primary dates/p/r/f distribution of schedules (P/n) and setup/servicing fees. 7 September - fully completed schedule + fee + amounts due, without payments.
			} catch (NoInitialDataException noDataException) {
				this.Error = noDataException.Message;
			} catch (InvalidInitialAmountException amountException) {
				this.Error = amountException.Message;
			} catch (InvalidInitialInterestRateException interestRateException) {
				this.Error = interestRateException.Message;
			} catch (InvalidInitialRepaymentCountException paymentsException) {
				this.Error = paymentsException.Message;
				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {
				this.Error = string.Format("Failed to get calculator instance/Schedule. customer {0}, err: {1}", model.CustomerID, ex.Message);
			} finally {
				if (nlCalculator == null) {
					this.Error = string.Format("Failed to get calculator instance. customer {0}, err: {1}", model.CustomerID);
				}
			}

			if (!string.IsNullOrEmpty(this.Error)) {
				Log.Alert("Failed to calculate Schedule. customer {0}, err: {1}", model.CustomerID, this.Error);
				return;
			}

			List<NL_LoanSchedules> nlSchedule = new List<NL_LoanSchedules>();
			List<NL_LoanFees> nlFees = new List<NL_LoanFees>();
			List<NL_LoanAgreements> nlAgreements = new List<NL_LoanAgreements>();
			NL_PacnetTransactions pacnetTransaction = null;
			DateTime nowTime = DateTime.UtcNow;

			// get updated history filled with Schedule
			history = model.Loan.LastHistory();

			Log.Debug(history);

			// copy to local schedules list
			history.Schedule.ForEach(s => nlSchedule.Add(s));

			if (nlSchedule.Count == 0) {
				this.Error += "Failed to generate Schedule or error occured during schedule/fees creation";
				return;
			}

			// 3. complete NL_Loans object data
			model.Loan = dataForLoan.Result.Loan;
			model.Loan.CreationTime = nowTime;
			model.Loan.LoanStatusID = (int)NLLoanStatuses.Live;
			model.Loan.Position += 1;

			Log.Debug("Adding loan: {0}", model.Loan);

			ConnectionWrapper pconn = DB.GetPersistent();

			try {

				pconn.BeginTransaction();

				// 4. save loan
				this.LoanID = DB.ExecuteScalar<long>(pconn, "NL_LoansSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", model.Loan));
				model.Loan.LoanID = this.LoanID;

				Log.Debug("NL_LoansSave: LoanID: {0}", this.LoanID);

				// 5. fees

				// copy to local fees list
				model.Loan.Fees.ForEach(f => nlFees.Add(f));

				// insert fees
				if (nlFees.Count > 0) {
					foreach (NL_LoanFees f in nlFees) {
						//if (f != null) {
							f.LoanID = this.LoanID; // set newly created LoanID
							f.CreatedTime = nowTime; // from calc-r
							f.AssignedByUserID = 1; //  from calc-r
						//}
					}

					nlFees.ForEach(f => Log.Debug("Adding fee: {0}", f));

					DB.ExecuteNonQuery(pconn, "NL_LoanFeesSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanFees>("Tbl", nlFees));
				}

				// 6. broker commissions
				// done in controller. When old loan removed: check if this is the broker's customer, calc broker fees, insert into LoanBrokerCommission
				if (model.Offer.BrokerSetupFeePercent > 0) {
					DB.ExecuteNonQuery(string.Format("UPDATE dbo.LoanBrokerCommission SET NLLoanID = {0} WHERE LoanID = {1}", this.LoanID, model.Loan.OldLoanID));
				}

				// 7. history
				history.LoanID = this.LoanID;
				history.Description = "adding loan " + this.LoanID + ", old ID: " + model.Loan.OldLoanID;

				Log.Debug("Adding history: {0}", history);

				history.LoanHistoryID = DB.ExecuteScalar<long>(pconn, "NL_LoanHistorySave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", history));

				Log.Debug("NL_LoanHistorySave: LoanID: {0}, LoanHistoryID: {1}", model.Loan.LoanID, history.LoanHistoryID);

				// 8. loan agreements
				history.Agreements.ForEach(a => nlAgreements.Add(a));
				nlAgreements.ForEach(a => a.LoanHistoryID = history.LoanHistoryID);

				nlAgreements.ForEach(a => Log.Debug("Adding agreement: {0}", a));

				DB.ExecuteNonQuery(pconn, "NL_LoanAgreementsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanAgreements>("Tbl", nlAgreements));

				// 9. schedules 
				nlSchedule.ForEach(s => s.LoanHistoryID = history.LoanHistoryID);

				nlSchedule.ForEach(s => Log.Debug("Adding schedule: {0}", s));

				DB.ExecuteNonQuery(pconn, "NL_LoanSchedulesSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanSchedules>("Tbl", nlSchedule));

				// 10. Fund Transfer 
				if (model.FundTransfer != null) {

					model.FundTransfer.LoanID = this.LoanID;
					model.FundTransfer.FundTransferID = DB.ExecuteScalar<long>(pconn, "NL_FundTransfersSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", model.FundTransfer));

					Log.Debug("NL_FundTransfersSave: LoanID: {0}, fundTransferID: {1}", this.LoanID, model.FundTransfer.FundTransferID);
				}

				pconn.Commit();

				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {

				pconn.Rollback();
				this.LoanID = 0;
				this.Error = ex.Message;
				Log.Error("Failed to add new loan: {0}", this.Error);
				
				SendMail("NL: loan rolled back", history, nlFees, nlSchedule, nlAgreements);
				
				return;
			}
			

			// 7. Pacnet transaction
			try {

				if (model.FundTransfer != null && (model.FundTransfer.PacnetTransactions != null && model.FundTransfer.FundTransferID > 0)) {

					pacnetTransaction = model.FundTransfer.LastPacnetTransactions();
					pacnetTransaction.FundTransferID = model.FundTransfer.FundTransferID;
					pacnetTransaction.PacnetTransactionID = DB.ExecuteScalar<long>("NL_PacnetTransactionsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", pacnetTransaction));

					Log.Debug("NL_PacnetTransactionsSave: LoanID: {0}, pacnetTransactionID: {1}", this.LoanID, pacnetTransaction.PacnetTransactionID);
				}

				// ReSharper disable once CatchAllClause
			} catch (Exception e1) {

				this.Error = e1.Message;
				Log.Error("Failed to save PacnetTransaction: {0}", this.Error);

				// PacnetTransaction error
				SendMail("NL: Failed to save PacnetTransaction", history, nlFees, nlSchedule, nlAgreements);
			}

			// OK
			SendMail("NL: Saved successfully", history, nlFees, nlSchedule, nlAgreements);

		}//Execute

			
		private void SendMail(string subject, NL_LoanHistory history, List<NL_LoanFees> fees, List<NL_LoanSchedules> schedule, List<NL_LoanAgreements> agreements) {

			string emailToAddress = CurrentValues.Instance.Environment.Value.Contains("Dev") ? "elinar@ezbob.com" : CurrentValues.Instance.EzbobTechMailTo;
			string emailFromName = CurrentValues.Instance.MailSenderName;
			string emailFromAddress = CurrentValues.Instance.MailSenderEmail;

			string sMsg = string.Format("{0}. cust {1} user {2}, oldloan {3}, LoanID {4} error: {5}", subject, model.CustomerID, model.UserID, model.Loan.OldLoanID, this.LoanID, this.Error);

			history.Schedule.Clear();
			history.Schedule = schedule;
			history.Agreements.Clear();
			history.Agreements = agreements;
			
			model.Loan.Histories.Clear();
			model.Loan.Histories.Add( history);
			model.Loan.Fees.Clear();
			model.Loan.Fees = fees;
			
			var message = string.Format(
				"<h5>{0}</h5>"
					+ "<h5>Loan</h5> <pre>{1}</pre>"
					+ "<h5>FundTransfer</h5> <pre>{2}</pre>"
					+ "<h5>PacnetTransaction</h5> <pre>{3}</pre>",

				HttpUtility.HtmlEncode(sMsg) 
				, HttpUtility.HtmlEncode(model.Loan.ToString())
				, HttpUtility.HtmlEncode(model.FundTransfer == null ? "no FundTransfer specified" : model.FundTransfer.ToString())
				//, HttpUtility.HtmlEncode(pacnetTransaction == null ? "no PacnetTransaction specified" : pacnetTransaction.ToString())
				);

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