namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using System.Threading.Tasks;
	using System.Web;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Backend.Strategies.Misc;
	using Ezbob.Backend.Strategies.NewLoan.Exceptions;
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

		public override void Execute() {

			DateTime nowTime = DateTime.UtcNow;

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

			if (history.EventTime == DateTime.MinValue) {
				this.Error = NL_ExceptionRequiredDataNotFound.HistoryEventTime;
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
				"NL_SignedOfferForLoan",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", model.CustomerID),
				new QueryParameter("@Now", history.EventTime)
			);

			if (dataForLoan.OfferID == 0) {
				this.Error = NL_ExceptionOfferNotValid.DefaultMessage;
				return;
			}

			if (dataForLoan.AvailableAmount < dataForLoan.LoanLegalAmount) {
				this.Error = "No available credit for current offer. New loan is not allowed."; // duplicate of ValidateAmount(loanAmount, cus); (loanAmount > customer.CreditSum)
				return;
			}

			if (dataForLoan.ExistsRefnums!=string.Empty && dataForLoan.ExistsRefnums.Contains(model.Loan.Refnum)) {
				this.Error = NL_ExceptionLoanExists.DefaultMessage;
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
			- agreements
			*/
		
			List<NL_LoanSchedules> nlSchedule = new List<NL_LoanSchedules>();
			List<NL_LoanFees> nlFees = new List<NL_LoanFees>();
			List<NL_LoanAgreements> nlAgreements = new List<NL_LoanAgreements>();

			ConnectionWrapper pconn = DB.GetPersistent();

			try {

				// 2. Init Schedule and Fees
				CalculateLoanSchedule scheduleStrategy = new CalculateLoanSchedule(model);
				scheduleStrategy.Context.UserID = model.UserID;
				scheduleStrategy.Execute();

				// get updated history filled with Schedule
				history = scheduleStrategy.Result.Loan.LastHistory();

				// copy to local schedules list
				history.Schedule.ForEach(s => nlSchedule.Add(s));

				if (nlSchedule.Count == 0 || scheduleStrategy.Result.Error != "") {
					this.Error = "Failed to generate Schedule or error occured during schedule/fees creation. " + scheduleStrategy.Result.Error;
					return;
				}

				// 3. complete NL_Loans object data
				model.Loan = scheduleStrategy.Result.Loan;
				model.Loan.CreationTime = nowTime;
				model.Loan.LoanStatusID = (int)NLLoanStatuses.Live;
				model.Loan.Position = ++dataForLoan.LoansCount;

				Log.Debug("Adding loan: {0}", model.Loan);

				pconn.BeginTransaction();

				// 4. save loan
				this.LoanID = DB.ExecuteScalar<long>(pconn, "NL_LoansSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", model.Loan));
				model.Loan.LoanID = this.LoanID;

				Log.Debug("NL_LoansSave: LoanID: {0}", this.LoanID);

				// 5. fees

				// copy to local fees list
				scheduleStrategy.Result.Loan.Fees.ForEach(f => nlFees.Add(f));

				if (nlFees.Count > 0) {
					foreach (NL_LoanFees f in nlFees) {
						if (f != null) {
							f.LoanID = model.Loan.LoanID; // set newly created LoanID
							f.CreatedTime = nowTime; // from calc-r
							f.AssignedByUserID = 1; //  from calc-r
						}
					}
					Log.Debug("Adding fees: ");
					nlFees.ForEach(f => Log.Debug(f));

					DB.ExecuteNonQuery(pconn, "NL_LoanFeesSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanFees>("Tbl", nlFees));
				}

				// 6. broker commissions
				// done in controller. When old loan removed: check if this is the broker's customer, calc broker fees, insert into LoanBrokerCommission
				if (scheduleStrategy.Result.BrokerComissions > 0) {
					DB.ExecuteNonQuery(string.Format("UPDATE dbo.LoanBrokerCommission SET NLLoanID = {0} WHERE LoanID = {1}", this.LoanID, model.Loan.OldLoanID));
				}

				// 7. history
				history.LoanID = this.LoanID;
				history.LoanLegalID = dataForLoan.LoanLegalID;
				history.Description = "adding loan " + this.LoanID + ", old ID: " + model.Loan.OldLoanID;
				history.AgreementModel = model.AgreementModel;

				Log.Debug("Adding history: {0}", history);

				history.LoanHistoryID = DB.ExecuteScalar<long>(pconn, "NL_LoanHistorySave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", history));

				Log.Debug("NL_LoanHistorySave: LoanID: {0}, LoanHistoryID: {1}", model.Loan.LoanID, history.LoanHistoryID);

				// 8. loan agreements
				AgreementModel agreementModel =  JsonConvert.DeserializeObject<AgreementModel>(model.AgreementModel);

				foreach (NLAgreementItem aItem in model.Agreements) {
					// prepare for NL_LoanAgreementsSave
					aItem.Agreement.LoanHistoryID = history.LoanHistoryID;
					nlAgreements.Add(aItem.Agreement);

					//item.Path1 = Path.Combine(CurrentValues.Instance.NL_AgreementPdfLoanPath1, item.Agreement.FilePath);
					//item.Path2 = Path.Combine(CurrentValues.Instance.NL_AgreementPdfConsentPath2, item.Agreement.FilePath);

					// saving to FS
					var item = aItem;
					//Task task = new Task(() => {
					new SaveAgreement(model.CustomerID, agreementModel, model.Loan.Refnum, "NLsavingAgreement", item.TemplateModel, item.Path1, item.Path2).Execute();
					//});
					//task.Start();
				}

				Log.Debug("Adding agreements:");
				nlAgreements.ForEach(a => Log.Debug(a));

				DB.ExecuteNonQuery(pconn, "NL_LoanAgreementsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanAgreements>("Tbl", nlAgreements));

				//int agreementSaveFS = await SaveAgreementsAsync();
				//Log.Debug("agreementSave to FS: {0}", agreementSaveFS);

				// 9. schedules 
				nlSchedule.ForEach(s => s.LoanHistoryID = history.LoanHistoryID);

				Log.Debug("Adding schedule:");
				nlSchedule.ForEach(s => Log.Debug(s));

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

				this.Error = string.Format("Failed to save new loan: {0}", ex);

				this.LoanID = 0;

				// general error
				SendMail(this.Error, history, nlFees, nlSchedule, nlAgreements, model.FundTransfer);

				Log.Error(this.Error);

				pconn.Rollback();
			}


			// 7. Pacnet transaction
			try {

				if (model.FundTransfer != null && (model.FundTransfer.PacnetTransactions != null && model.FundTransfer.FundTransferID > 0)) {

					NL_PacnetTransactions transaction = model.FundTransfer.LastPacnetTransactions();

					transaction.FundTransferID = model.FundTransfer.FundTransferID;

					transaction.PacnetTransactionID = DB.ExecuteScalar<int>("NL_PacnetTransactionsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", transaction));

					Log.Debug("NL_PacnetTransactionsSave: LoanID: {0}, pacnetTransactionID: {1}", this.LoanID, transaction.PacnetTransactionID);
				}

				// ReSharper disable once CatchAllClause
			} catch (Exception e1) {

				this.Error = string.Format("Failed to save NL PacnetTransaction: {0}", e1);

				// PacnetTransaction error
				SendMail(this.Error, history, nlFees, nlSchedule, nlAgreements, model.FundTransfer);

				Log.Error(this.Error);
				
			}

			// OK
			SendMail("Saved successfully", history, nlFees, nlSchedule, nlAgreements, model.FundTransfer);

		}//Execute


		//private async Task<int> SaveAgreementsAsync() {
		//	try {
		//		// save agreements to file system
		//		if (model.AgreementModel == null) {
		//			Log.Alert("AgreementModel not exists on creating Nloan");
		//			this.Error = "AgreementModel not exists on creating Nloan";
		//			return 0;
		//		}

		//		AgreementModel agreementModel =  JsonConvert.DeserializeObject<AgreementModel>(model.AgreementModel);
		//		foreach (NLAgreementItem  item in model.Agreements) {
		//			SaveAgreement saveAgreement = new SaveAgreement(customerId: model.CustomerID, model: agreementModel, refNumber: model.Loan.Refnum, name: "savingAgreement", template: item.TemplateModel, path1: item.Path1, path2: item.Path2);
		//			saveAgreement.Execute();
		//		}

		//		return await Task.FromResult(1);

		//		// ReSharper disable once CatchAllClause
		//	} catch (Exception exSaveAgreement) {
		//		Log.Debug("Failed to save agreements to FS. {0}", exSaveAgreement);
		//		return 0;
		//	}
		//}

		private void SendMail(string sMsg, NL_LoanHistory history = null, List<NL_LoanFees> fees = null, List<NL_LoanSchedules> schedule = null, List<NL_LoanAgreements> agreements = null, 
			NL_FundTransfers fundTransfer = null, NL_PacnetTransactions  pacnetTransaction = null) {

			string emailToAddress = CurrentValues.Instance.Environment.Value.Contains("Dev") ? "elinar@ezbob.com" : CurrentValues.Instance.EzbobTechMailTo;
			string emailFromName = CurrentValues.Instance.MailSenderName;
			string emailFromAddress = CurrentValues.Instance.MailSenderEmail;

			StringBuilder sb = new StringBuilder();
			string strSchedule = "no Schedule specified";
			if (schedule != null) {
				schedule.ForEach(s => sb.Append(s));
				strSchedule = sb.ToString();
			}

			sb = new StringBuilder();
			string strFees = "no LoanFees specified";
			if (fees != null) {
				fees.ForEach(f => sb.Append(f));
				strFees = sb.ToString();
			}

			sb = new StringBuilder();
			string strAgreements = "no LoanAgreements specified";
			if (agreements != null) {
				agreements.ForEach(a => sb.Append(a));
				strAgreements = sb.ToString();
			}

			string subject = string.Format("New loan adding: CustomerID: {0}, UserID: {1}, old loanID: {2}, LoanID: {3}", model.CustomerID, model.UserID, model.Loan.OldLoanID, this.LoanID);

			//- loan
			//- fees
			//- history
			//- schedules
			//- broker comissions ? - update NLLoanID
			//- fund transfer
			//- pacnet transaction

			var message = string.Format(
						"<h5>{0}</h5>"
						+ "<h5>Loan</h5> {1}"
						+ "<h5>History</h5> {2}"
						+ "<h5>Fees</h5> {3}"
						+ "<h5>Schedules</h5> {4}"
						+ "<h5>Agreements</h5> {5}"
						+ "<h5>Transfer</h5> {6}"
						+ "<h5>PacnetTransaction</h5> {7}",

						HttpUtility.HtmlEncode(sMsg)
					, HttpUtility.HtmlEncode(model.Loan == null ? "no Loan specified" : model.Loan.ToString())
					, HttpUtility.HtmlEncode(history == null ? "no LoanHistory specified" : history.ToString())
					, HttpUtility.HtmlEncode(strFees)
					, HttpUtility.HtmlEncode(strSchedule) 
					, HttpUtility.HtmlEncode(strAgreements)
					, HttpUtility.HtmlEncode(fundTransfer == null ? "no FundTransfer specified" : fundTransfer.ToString())
					, HttpUtility.HtmlEncode(pacnetTransaction == null ? "no PacnetTransaction specified" : pacnetTransaction.ToString()));

			new Mail().Send(
				  emailToAddress,
				  null, // message text
				  message, //html
				  emailFromAddress, // fromEmail
				  emailFromName, // fromName
				  subject
			);

		} // SendMail



	}//class AddLoan
}//ns