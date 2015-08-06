namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Threading.Tasks;
	using System.Web;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Backend.Models.NewLoan;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Backend.Strategies.Misc;
	using Ezbob.Database;
	using EzBob.Backend.Models;
	using MailApi;
	using Newtonsoft.Json;

	public class AddLoan : AStrategy {

		public AddLoan(NL_Model nlModel) {
			model = nlModel;

			if (CurrentValues.Instance.Environment.Value.Contains("dev")) {
				this.emailToAddress = "elinar@ezbob.com";
				this.emailFromAddress = "elinar@ezbob.com";
			} else {
				this.emailToAddress = CurrentValues.Instance.EzbobTechMailTo;
				this.emailFromAddress = "elinar@ezbob.com";
			}
			this.emailFromName = "ezbob-system";
		}//constructor

		public override string Name { get { return "AddLoan"; } }
		public NL_Model Result; // output
		
		public override async void Execute() {

			string message;

			if (model.CustomerID == 0) {
				this.Result.Error = NL_ExceptionCustomerNotFound.DefaultMessage;
				return;
			}

			// check if "creadit available" is enough for this loan amount
			// loan for the offer exists
			/*SafeReader sr = DB.GetFirst(string.Format("SELECT LoanID FROM NL_Loans WHERE OfferID={0}", dataForLoan.OfferID));
			if (sr["LoanID"] > 0) {
				  message = string.Format("Loan for customer {0} and offer {1} exists", model.CustomerID, dataForLoan.OfferID);
				  Log.Alert(message);
				  throw new NL_ExceptionLoanExists(message);
			}*/

			// input validation
			if (model.Loan == null) {
				this.Result.Error = string.Format("Expected input data not found (NL_Model initialized by: Loan.OldLoanID, Loan.Refnum). Customer {0}", model.CustomerID);
				return;
			}

			// for Alibaba and Everline - money should'n be transferred
			//if (model.FundTransfer == null) {
			//	this.Result.Error = string.Format("Expected input data not found (NL_Model initialized by: FundTransfer). Customer {0}", model.CustomerID);
			//	return;
			//}

			if (model.Agreements == null || model.Agreements.Count == 0) {
				this.Result.Error = string.Format("Expected input data not found (NL_Model initialized by: NLAgreementItem list). Customer {0}", model.CustomerID);
				return;
			}

			if (model.AgreementModel == null) {
				this.Result.Error = string.Format("Expected input data not found (NL_Model initialized by: AgreementModel in JSON). Customer {0}", model.CustomerID);
				return;
			}

			OfferForLoan dataForLoan = DB.FillFirst<OfferForLoan>("NL_OfferForLoan", CommandSpecies.StoredProcedure, new QueryParameter("CustomerID", model.CustomerID), new QueryParameter("@Now", model.Loan.IssuedTime));

			if (dataForLoan == null) {
				this.Result.Error = NL_ExceptionOfferNotValid.DefaultMessage;
				return;
			}

			if (dataForLoan.AvailableAmount < dataForLoan.LoanLegalAmount) {
				this.Result.Error = "No available credit for current offer. New loan is not allowed."; // duplicate of ValidateAmount(loanAmount, cus); (loanAmount > customer.CreditSum)
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

			// TODO check usage of DefaultRepaymentPeriod from LoanSource
			// TODO additional check to be replicated here in master, commits 
			// adding safe check before creating loan that the interest rate is correct 4b6d139658203863dc57cc13748ea49da498ff27
			// add server side validation of repayment period when CreateLoan  b6926a4428919dd0343dd4f8c7ceb0d5d6b1b03d

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
			NL_LoanHistory history = null;
			List<NL_LoanAgreements> agreements = new List<NL_LoanAgreements>();

			DateTime nowTime = DateTime.UtcNow;

			ConnectionWrapper pconn = DB.GetPersistent();

			try {

				// 1. set required data for Schedule and Fees init
				model.Loan.IssuedTime = nowTime;

				// 2. Init Schedule and Fees
				CalculateLoanSchedule scheduleAndFees = new CalculateLoanSchedule(model);
				scheduleAndFees.Context.UserID = model.UserID;
				scheduleAndFees.Execute();
				if (scheduleAndFees.Result.Schedule == null || scheduleAndFees.Result.Error != "") {
					this.Result.Error = "Failed to generate Schedule or error occured during schedule/fees creation. " + scheduleAndFees.Result.Error;
					return;
				}

				// 3. complete NL_Loans object data
				model.Loan = scheduleAndFees.Result.Loan;
				model.Loan.CreationTime = nowTime;
				model.Loan.LoanStatusID = (int)NLLoanStatuses.Live;

				Log.Debug("Adding loan: {0}", model.Loan);

				pconn.BeginTransaction();

				// 4. save loan
				this.LoanID = DB.ExecuteScalar<long>(pconn, "NL_LoansSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", model.Loan));

				Log.Debug("NL_LoansSave: LoanID: {0}", this.LoanID);

				// 5. fees
				if (scheduleAndFees.Result.Fees != null) {

					scheduleAndFees.Result.Fees.ForEach(f => fees.Add(f.Fee));
					fees.ForEach(f => f.LoanID = this.LoanID); // set newly created LoanID

					Log.Debug("Adding fees: "); fees.ForEach(f => Log.Debug(f));

					DB.ExecuteNonQuery(pconn, "NL_LoanFeesSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanFees>("Tbl", fees));
				}

				// 6. broker comissions
				// done in controller. When old loan removed: check if this is the broker's customer, calc broker fees, insert into LoanBrokerCommission
				if (scheduleAndFees.Result.BrokerComissions > 0) {
					DB.ExecuteNonQuery(string.Format("UPDATE dbo.LoanBrokerCommission SET NLLoanID = {0} WHERE LoanID = {1}", this.LoanID, model.Loan.OldLoanID));
				}

				// 7. history
				history = new NL_LoanHistory() {
					LoanID = this.LoanID,
					UserID = model.UserID,
					LoanLegalID = dataForLoan.LoanLegalID,
					Amount = model.Loan.InitialLoanAmount,
					RepaymentCount = model.Loan.RepaymentCount,
					InterestRate = model.Loan.InterestRate,
					EventTime = nowTime,
					Description = "add loan ID " + this.LoanID,
					AgreementModel = model.AgreementModel
				};

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
				scheduleAndFees.Result.Schedule.ForEach(s => schedule.Add(s.ScheduleItem));
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


		}//Execute


		private async Task<int> SaveAgreementsAsync() {
			try {
				// save agreements to file system
				if (model.AgreementModel == null) {
					Log.Alert("AgreementModel not exists on creating Nloan");
					this.Result.Error = "AgreementModel not exists on creating Nloan";
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