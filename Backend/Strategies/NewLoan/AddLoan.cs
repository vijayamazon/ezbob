namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Web;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Backend.Strategies.Misc;
	using Ezbob.Database;
	using EzBob.Backend.Models;
	using MailApi;
	using Newtonsoft.Json;

	public class AddLoan : AStrategy {

		/// <summary>
		/// Create new loan
		/// Expected input:
		///		NL_Model with:
		///			- Loan.OldLoanID
		///			- Loan.Refnum
		///			- Histories => history: EventTime (former IssuedTime)
		///			- Agreements
		///			- AgreementModel (JSON)
		///		Expected result:
		///			- int LoanID newly created
		///			- optional string Error
		/// </summary>
		/// <param name="nlModel"></param>
		public AddLoan(NL_Model nlModel) {
			model = nlModel;

			this.emailToAddress = CurrentValues.Instance.Environment.Value.Contains("dev") ? "elinar@ezbob.com" : CurrentValues.Instance.EzbobTechMailTo;

			this.emailFromName = "ezbob-dev-system";
			this.emailFromName = CurrentValues.Instance.MailSenderName;
			this.emailFromAddress = CurrentValues.Instance.MailSenderEmail;

		}//constructor

		public override string Name { get { return "AddLoan"; } }
		public string Error; // output
		
		public override async void Execute() {

			DateTime nowTime = DateTime.UtcNow;

			string message;

			// TODO check if "credit available" is enough for this loan amount

			if (model.CustomerID == 0) {
				this.Error = NL_ExceptionCustomerNotFound.DefaultMessage;
				return;
			}

			var history = model.Histories.OrderBy(h => h.EventTime).LastOrDefault();

			if (history == null) {
				this.Error = NL_ExceptionRequiredDataNotFound.LastHistory; //"Expected input data not found (NL_Model initialized by: Histories.NL_LoanHistory.EventTime)";
				return;
			}

			if (history.EventTime == DateTime.MinValue) {
				this.Error = NL_ExceptionRequiredDataNotFound.HistoryEventTime;
				return;
			}
		
			if (model.Loan == null) {
				this.Error = NL_ExceptionRequiredDataNotFound.Loan; //string.Format("Expected input data not found (NL_Model initialized by: Loan.OldLoanID, Loan.Refnum). Customer {0}", model.CustomerID);
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

			if (model.Agreements == null || model.Agreements.Count == 0) {
				this.Error = string.Format("Expected input data not found (NL_Model initialized by: NLAgreementItem list). Customer {0}", model.CustomerID);
				return;
			}

			if (model.AgreementModel == null) {
				this.Error = string.Format("Expected input data not found (NL_Model initialized by: AgreementModel in JSON). Customer {0}", model.CustomerID);
				return;
			}
			
			// valid offer
			OfferForLoan dataForLoan = DB.FillFirst<OfferForLoan>(
				"NL_OfferForLoan",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", model.CustomerID),
				new QueryParameter("@Now", lastHistory.EventTime)
			);

			if (dataForLoan == null) {
				this.Error = NL_ExceptionOfferNotValid.DefaultMessage;
				return;
			}

			if (dataForLoan.AvailableAmount < dataForLoan.LoanLegalAmount) {
				this.Error = "No available credit for current offer. New loan is not allowed."; // duplicate of ValidateAmount(loanAmount, cus); (loanAmount > customer.CreditSum)
				return;
			}

			Log.Debug(dataForLoan.ToString());

			// complete other validations here

			/*** 
			//CHECK "Enough Funds" (uncomment WHEN old BE REMOVED from \App\PluginWeb\EzBob.Web\Code\LoanCreator.cs, method CreateLoan method)                    
			VerifyEnoughAvailableFunds enoughAvailableFunds = new VerifyEnoughAvailableFunds(model.Loan.InitialLoanAmount);
			enoughAvailableFunds.Execute();
			if (!enoughAvailableFunds.HasEnoughFunds) {
				  Log.Alert("No enough funds for loan: customer {0}; offer {1}", model.CustomerID, dataForLoan.Offer.OfferID);
			}
			****/
			
			/**
			- loan
			- fees
			- history
			- schedules
			- broker comissions ? - update NLLoanID
			- fund transfer
			- pacnet transaction
			*/

			long fundTransferID = 0;
			List<NL_LoanSchedules> schedule = new List<NL_LoanSchedules>();
			List<NL_LoanFees> fees = new List<NL_LoanFees>();
			List<NL_LoanAgreements> agreements = new List<NL_LoanAgreements>();

			ConnectionWrapper pconn = DB.GetPersistent();

			try {

				// 2. Init Schedule and Fees
				CalculateLoanSchedule scheduleStrategy = new CalculateLoanSchedule(model);
				scheduleStrategy.Context.UserID = model.UserID;
				scheduleStrategy.Execute();

				schedule = scheduleStrategy.Result.Schedule.OfType<NL_LoanSchedules>().ToList();

				if (schedule.Count==0 || scheduleStrategy.Result.Error != "") {
					this.Error = "Failed to generate Schedule or error occured during schedule/fees creation. " + scheduleStrategy.Result.Error;
					return;
				}

				// 3. complete NL_Loans object data
				model.Loan = scheduleStrategy.Result.Loan;
				model.Loan.CreationTime = nowTime;
				model.Loan.LoanStatusID = (int)NLLoanStatuses.Live;

				Log.Debug("Adding loan: {0}", model.Loan);

				pconn.BeginTransaction();

				// 4. save loan
				this.LoanID = DB.ExecuteScalar<long>(pconn, "NL_LoansSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", model.Loan));

				Log.Debug("NL_LoansSave: LoanID: {0}", this.LoanID);

				fees = scheduleStrategy.Result.Fees.OfType<NL_LoanFees>().ToList();

				// 5. fees
				if (fees.Count > 0) {

					foreach (NL_LoanFees f in fees) {
						f.LoanID = this.LoanID; // set newly created LoanID
						f.CreatedTime = nowTime;
						f.AssignedByUserID = 1;
					}
					
					Log.Debug("Adding fees: "); fees.ForEach(f => Log.Debug(f));

					DB.ExecuteNonQuery(pconn, "NL_LoanFeesSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanFees>("Tbl", fees));
				}

				// 6. broker commissions
				// done in controller. When old loan removed: check if this is the broker's customer, calc broker fees, insert into LoanBrokerCommission
				if (scheduleStrategy.Result.BrokerComissions > 0) {
					DB.ExecuteNonQuery(string.Format("UPDATE dbo.LoanBrokerCommission SET NLLoanID = {0} WHERE LoanID = {1}", this.LoanID, model.Loan.OldLoanID));
				}

				// 7. history
				history.LoanID = this.LoanID;
				//history.UserID = model.UserID;
				history.LoanLegalID = dataForLoan.LoanLegalID;
				history.Description = "adding loan " + this.LoanID + ", old ID: " + model.Loan.OldLoanID;
				history.AgreementModel = model.AgreementModel;
				
				Log.Debug("Adding history: {0}", history);

				long historyID = DB.ExecuteScalar<long>(pconn, "NL_LoanHistorySave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", history));

				Log.Debug("NL_LoanHistorySave: LoanID: {0}, LoanHistoryID: {1}", this.LoanID, historyID);

				// 8. loan agreements
				foreach (NLAgreementItem item in model.Agreements) {
					item.Agreement.LoanHistoryID = historyID;
					agreements.Add(item.Agreement);

					item.Path1 = Path.Combine(CurrentValues.Instance.NL_AgreementPdfLoanPath1, item.Agreement.FilePath);
					item.Path2 = Path.Combine(CurrentValues.Instance.AgreementPdfLoanPath2, item.Agreement.FilePath);
				}

				Log.Debug("Adding agreements:"); agreements.ForEach(a => Log.Debug(a));

				DB.ExecuteNonQuery(pconn, "NL_LoanAgreementsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanAgreements>("Tbl", agreements));

				int agreementSaveFS = await SaveAgreementsAsync();
				Log.Debug("agreementSave to FS: {0}", agreementSaveFS);

				// 9. schedules 
				schedule.ForEach(s => s.LoanHistoryID = historyID);

				Log.Debug("Adding schedule:"); schedule.ForEach(s => Log.Debug(s));

				DB.ExecuteNonQuery(pconn, "NL_LoanSchedulesSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanSchedules>("Tbl", schedule));

				// 10. Fund Transfer 
				if (model.FundTransfer != null) {

					model.FundTransfer.LoanID = this.LoanID;

					fundTransferID = DB.ExecuteScalar<long>(pconn, "NL_FundTransfersSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", model.FundTransfer));

					Log.Debug("NL_FundTransfersSave: LoanID: {0}, fundTransferID: {1}", this.LoanID, fundTransferID);
				}

				pconn.Commit();

				

				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {

				message = string.Format("Failed to create NL_Loan for customer {0}, oldLoanID {1}, err: {2}", model.CustomerID, model.Loan.OldLoanID, ex);
				this.LoanID = 0;
				pconn.Rollback();

				Log.Error(message);
				SendMail(message, fees, schedule, history, agreements);
			}


			// 7. Pacnet transaction
			try {

				if (model.PacnetTransaction != null && fundTransferID > 0) {

					model.PacnetTransaction.FundTransferID = fundTransferID;

					List<NL_PacnetTransactionStatuses> pacnetStatuses = NL_Loader.PacnetTransactionStatuses();
					var transactionStatus = pacnetStatuses.Find(s => s.TransactionStatus.Equals(model.PacnetTransactionStatus)) ?? pacnetStatuses.Find(s => s.TransactionStatus.Equals("Unknown"));

					model.PacnetTransaction.PacnetTransactionStatusID = transactionStatus.PacnetTransactionStatusID;

					model.PacnetTransaction.PacnetTransactionID = DB.ExecuteScalar<int>("NL_PacnetTransactionsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", model.PacnetTransaction));

					Log.Debug("NL_PacnetTransactionsSave: LoanID: {0}, pacnetTransactionID: {1}", this.LoanID, model.PacnetTransaction.PacnetTransactionID);
				}

				// ReSharper disable once CatchAllClause
			} catch (Exception e1) {
				message = string.Format("Failed to write NL PacnetTransaction: Customer {0}, oldLoanID {1}, err: {2}", model.CustomerID, model.Loan.OldLoanID, e1);
				SendMail(message, fees, schedule, history, agreements);
				Log.Error(message);
			}

			SendMail("", fees, schedule, history, agreements);

		}//Execute


		private async Task<int> SaveAgreementsAsync() {
			try {
				// save agreements to file system
				if (model.AgreementModel == null) {
					Log.Alert("AgreementModel not exists on creating Nloan");
					this.Error = "AgreementModel not exists on creating Nloan";
					return 0;
				}

				AgreementModel agreementModel =  JsonConvert.DeserializeObject<AgreementModel>(model.AgreementModel);
				foreach (NLAgreementItem  item in model.Agreements) {
					SaveAgreement saveAgreement = new SaveAgreement(customerId: model.CustomerID, model: agreementModel, refNumber: model.Loan.Refnum, name: "savingAgreement", template: item.TemplateModel, path1: item.Path1, path2: item.Path2);
					saveAgreement.Execute();
				}

				return await Task.FromResult(1);

				// ReSharper disable once CatchAllClause
			} catch (Exception exSaveAgreement) {
				Log.Debug("Failed to save agreements to FS. {0}", exSaveAgreement);
				return 0;
			}
		}

		private void SendMail(string sMsg, List<NL_LoanFees> fees = null, List<NL_LoanSchedules> scheduleItems = null, NL_LoanHistory history = null, List<NL_LoanAgreements> agreements = null, int fundTransferID = 0) {

			var message = string.Format(
					"<h5>CustomerID: {0}; UserID: {1}</h5>, Message: {2}<p>"
						+ "<h5>NL_Loan</h5>: {3} "
						+ "<h5>NL_LoanHistory</h5>: {4} "
						+ "<h5>NL_LoanFees</h5>: {5} "
						+ "<h5>NL_LoanSchedules</h5>: {6} "
						+ "<h5>NL_LoanAgreements</h5>: {7} "
						+ "<h5>NL_FundTransfer</h5>: {8} "
						+ "<h5>NL_PacnetTransaction</h5>: {9}",

					model.CustomerID, model.UserID, sMsg
					, HttpUtility.HtmlEncode(model.Loan == null ? "no Loan specified" : model.Loan.ToString())
					, HttpUtility.HtmlEncode(history == null ? "no LoanHistory specified" : history.ToString())
					, HttpUtility.HtmlEncode(fees == null ? "no LoanFees specified" : fees.ToString())
					, HttpUtility.HtmlEncode(scheduleItems == null ? "no Schedule specified" : scheduleItems.ToString()) // NL_LoanSchedules
					, HttpUtility.HtmlEncode(agreements == null ? "no LoanAgreements specified" : agreements.ToString()) // NL_LoanAgreements
					, HttpUtility.HtmlEncode(model.FundTransfer == null ? "no FundTransfer specified" : model.FundTransfer + ", fundTransferID: " + fundTransferID)
					, HttpUtility.HtmlEncode(model.PacnetTransaction == null ? "no PacnetTransaction specified" : model.PacnetTransaction.ToString()));

			new Mail().Send(
				  this.emailToAddress,
				  null, // message text
				  message, //html
				  this.emailFromAddress, // fromEmail
				  this.emailFromName, // fromName
				  sMsg //"#NL_Loan failed oldLoanID: " + (int)model.Loan.OldLoanID + " for customer " + model.CustomerID // subject
			);

		} // SendMail

		public long LoanID;
		public NL_Model model { get; private set; }

		private readonly string emailToAddress;
		private readonly string emailFromAddress;
		private readonly string emailFromName;

	}//class AddLoan
}//ns