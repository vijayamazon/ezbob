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
	using Ezbob.Backend.Strategies.NewLoan.Migration;
	using Ezbob.Database;
	using MailApi;
	using PaymentServices.Calculators;

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
		/// Mail notifications sent on success/on error
		/// </summary>
		/// <param name="nlModel"></param>
		public AddLoan(NL_Model nlModel) {
			model = nlModel;
			nowTime = DateTime.UtcNow;
			this.strategyArgs = new object[] { model };
		}

		public override string Name { get { return "AddLoan"; } }
		public string Error { get; private set; }
		public long LoanID { get; private set; }
		public NL_Model model { get; private set; }

		private readonly object[] strategyArgs;

		public DateTime nowTime { get; private set; }

		
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

			if (!CurrentValues.Instance.NewLoanRun) {
				NL_AddLog(LogType.Info, "NL disabled by configuration", null, null, null, null);
				return;
			}

			NL_AddLog(LogType.Info, "Strategy Start", this.strategyArgs, Error, null, null);

			try {
				if (model.CustomerID == 0) {
					Error = NL_ExceptionCustomerNotFound.DefaultMessage;
					NL_AddLog(LogType.Error, "Strategy failed", this.strategyArgs, null, Error, null);
					return;
				}

				if (model.Loan == null) {
					Error = NL_ExceptionRequiredDataNotFound.Loan;
					NL_AddLog(LogType.Error, "Strategy failed", this.strategyArgs, null, Error, null);
					return;
				}

				if (model.Loan.OldLoanID == null) {
					Error = NL_ExceptionRequiredDataNotFound.OldLoan;
					NL_AddLog(LogType.Error, "Strategy failed", this.strategyArgs, null, Error, null);
					return;
				}

				var history = model.Loan.LastHistory();

				if (history == null) {
					Error = NL_ExceptionRequiredDataNotFound.LastHistory;
					NL_AddLog(LogType.Error, "Strategy failed", this.strategyArgs, null, Error, null);
					return;
				}

				if (history.Agreements == null || history.Agreements.Count == 0) {
					Error = string.Format("Expected input data not found (NL_Model initialized by: NLAgreementItem list). Customer {0}", model.CustomerID);
					NL_AddLog(LogType.Error, "Strategy failed", this.strategyArgs, null, Error, null);
					return;
				}

				if (string.IsNullOrEmpty(history.AgreementModel)) {
					Error = string.Format("Expected input data not found (NL_Model initialized by: AgreementModel in JSON). Customer {0}", model.CustomerID);
					NL_AddLog(LogType.Error, "Strategy failed", this.strategyArgs, null, Error, null);
					return;
				}

				BuildLoanFromOffer dataForLoan = new BuildLoanFromOffer(model);
				try {
					dataForLoan.Execute();
					// ReSharper disable once CatchAllClause
				} catch (Exception ex) {
					Log.Alert(ex.Message);
				}

				if (!string.IsNullOrEmpty(dataForLoan.Error)) {
					Error = dataForLoan.Error;
					NL_AddLog(LogType.DataExsistense, "Strategy failed - Failed to generate Schedule/fees", this.strategyArgs, null, Error, null);
					return;
				}

				model = dataForLoan.Result;

				// prevent to create the same loan (by refnum)
				if (!string.IsNullOrEmpty(model.Loan.Refnum) && !string.IsNullOrEmpty(dataForLoan.DataForLoan.ExistsRefnums) && dataForLoan.DataForLoan.ExistsRefnums.Contains(model.Loan.Refnum)) {
					Error = NL_ExceptionLoanExists.DefaultMessage;
					NL_AddLog(LogType.Info, "Strategy End", this.strategyArgs, null, Error, null);
					return;
				}

				// setup/distributed fees

				// for now: only one-time or "spreaded" setup fees supported
				// add full fees 2.0 support later

				var offerFees = model.Offer.OfferFees;

				// don't create LoanFees if OfferFees Percent == 0 or AbsoluteAmount == 0
				var setupFee = offerFees.FirstOrDefault(f => f.LoanFeeTypeID == (int)NLFeeTypes.SetupFee && (f.Percent > 0 || f.AbsoluteAmount > 0));
				var servicingFee = offerFees.FirstOrDefault(f => f.LoanFeeTypeID == (int)NLFeeTypes.ServicingFee && (f.Percent > 0 || f.AbsoluteAmount > 0)); // equal to "setup spreaded"
				decimal? brokerFeePercent = model.Offer.BrokerSetupFeePercent;
			 
				var feeCalculator = new SetupFeeCalculator(setupFee!=null ?setupFee.Percent: servicingFee.Percent, brokerFeePercent);
				decimal setupFeeAmount = feeCalculator.Calculate(history.Amount);
				SetupFeeCalculator.AbsoluteFeeAmount ff = feeCalculator.CalculateTotalAndBroker(history.Amount);
				model.BrokerComissions = ff.Broker;

				// send ot Calculator to distribute and attach to schedule planned dates
				history.DistributedFees = servicingFee == null ? 0: setupFeeAmount;
				
				ALoanCalculator nlCalculator = new LegacyLoanCalculator(model);
				// 2. Init Schedule and Fees
				try {
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
					NL_AddLog(LogType.Error, "Strategy failed", this.strategyArgs, null, Error, ex.StackTrace);
					return;
				}

				//// prevent creation of same loan
				//if (!string.IsNullOrEmpty(Error)) {
				//	Log.Info("Failed to calculate Schedule. customer {0}, err: {1}", model.CustomerID, Error);
				//	NL_AddLog(LogType.Error, "Strategy " + string.Format("Failed to calculate Schedule. customer {0}, err: {1}", model.CustomerID, Error), this.strategyArgs, null, Error, null);
				//	return;
				//}

				history.OutstandingInterest = nlCalculator.Interest;

				List<NL_LoanSchedules> nlSchedule = new List<NL_LoanSchedules>();
				List<NL_LoanFees> nlFees = new List<NL_LoanFees>();
				List<NL_LoanAgreements> nlAgreements = new List<NL_LoanAgreements>();

				// get updated history filled with Schedule
				history = model.Loan.LastHistory();

				// copy to local schedules list
				history.Schedule.ForEach(s => nlSchedule.Add(s));

				if (nlSchedule.Count == 0) {
					Error += "Failed to generate Schedule/fees";
					NL_AddLog(LogType.Info, "Strategy failed", this.strategyArgs, null, Error, null);
					return;
				}

				// 3. complete NL_Loans object data
				model.Loan = dataForLoan.Result.Loan;
				model.Loan.CreationTime = nowTime;
				model.Loan.LoanStatusID = (int)NLLoanStatuses.Live;
				model.Loan.Position += 1;
				
				ConnectionWrapper pconn = DB.GetPersistent();

				try {

					pconn.BeginTransaction();

					// 4. save loan
					LoanID = DB.ExecuteScalar<long>(pconn, "NL_LoansSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", model.Loan));
					model.Loan.LoanID = LoanID;

					//Log.Debug("NL_LoansSave: LoanID: {0}", this.LoanID);

					// 5. fees
					// copy to local fees list
					model.Loan.Fees.ForEach(f => nlFees.Add(f));

					foreach (NL_LoanFees f in nlFees) {
						f.CreatedTime = nowTime; // from calc-r
						f.AssignedByUserID = 1; //  from calc-r
						f.LoanID = LoanID;
					}

					// setup as fee
					if (setupFee != null) {
						Log.Debug("setupFeeAmount: {0}", setupFeeAmount);
						nlFees.Add(
							new NL_LoanFees() {
								LoanID = LoanID,
								Amount = setupFeeAmount,
								AssignTime = history.EventTime,
								Notes = "setup fee one-part",
								LoanFeeTypeID = (int)NLFeeTypes.SetupFee,
								CreatedTime = nowTime,
								AssignedByUserID = 1
							});
					}

					nlFees.ForEach(f => Log.Debug("Adding fees: {0}", f));

					// insert fees
					DB.ExecuteNonQuery(pconn, "NL_LoanFeesSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanFees>("Tbl", nlFees));

					model.Loan.Fees.Clear();
					model.Loan.Fees.AddRange(nlFees);

					// 7. history
					history.LoanID = LoanID;
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
						model.FundTransfer.LoanID = LoanID;
						model.FundTransfer.FundTransferID = DB.ExecuteScalar<long>(pconn, "NL_FundTransfersSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", model.FundTransfer));
						//Log.Debug("NL_FundTransfersSave: LoanID: {0}, fundTransferID: {1}", this.LoanID, model.FundTransfer.FundTransferID);
					}

					// 11. save default loan options record
					model.Loan.LoanOptions.LoanOptionsID = DB.ExecuteScalar<long>(pconn, "NL_LoanOptionsSave",
					   CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", new NL_LoanOptions {
						   LoanID = LoanID,
						   UserID = 1, // default system user?
						   InsertDate = nowTime,
						   IsActive = true,
						   Notes = "default options"
					   }), new QueryParameter("@LoanID", LoanID)
					 );

					pconn.Commit();

					// ReSharper disable once CatchAllClause
				} catch (Exception ex) {

					pconn.Rollback();

					LoanID = 0;
					Error = ex.Message;
					Log.Error("Failed to add new loan: {0}", Error);

					SendMail("NL: loan rolled back", history, nlFees, nlSchedule, nlAgreements);

					NL_AddLog(LogType.Error, "Strategy failed - Failed to add new loan", this.strategyArgs, Error, ex.ToString(), ex.StackTrace);
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
					SendMail("NL: Failed to save PacnetTransaction", history, nlFees, nlSchedule, nlAgreements);
				}

				// 11. if setup fee - add payment to offset it
				SetupOffsetPayment();
				
				// 6. broker commissions
				// done in controller. When old loan removed: check if this is the broker's customer, calc broker fees, insert into LoanBrokerCommission
				if (model.Offer.BrokerSetupFeePercent > 0) {
					DB.ExecuteNonQuery(string.Format("UPDATE dbo.LoanBrokerCommission SET NLLoanID = {0} WHERE LoanID = {1}", LoanID, model.Loan.OldLoanID));
				}

				// OK
				SendMail("NL: Saved successfully", history, nlFees, nlSchedule, nlAgreements);

				// copy LoanCharges Ids into OldFeeID, NL_LoanFees
				DB.ExecuteNonQuery("NL_LoanFeesOldIDUpdate", CommandSpecies.StoredProcedure);

				// temporary - should be removed/modified after "old" loan remove
				//CopyRebateTransaction();

				MigrateLoan sMigrateLoan = new MigrateLoan();
				try {
					sMigrateLoan.Execute();
					// ReSharper disable once CatchAllClause
				} catch (Exception mex) {
					Error = mex.Message;
					Log.Error("Failed sync migration: {0}", Error);
					NL_AddLog(LogType.Error, "Failed sync migration", this.strategyArgs, Error, mex.ToString(), mex.StackTrace);
				}

				NL_AddLog(LogType.Info, "Strategy End", this.strategyArgs, LoanID, Error, null);

				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {
				NL_AddLog(LogType.Error, "Strategy failed", this.strategyArgs, Error, ex.ToString(), ex.StackTrace);
			}
		}//Execute

		// 11. if setup fee - add payment to offset it
		private void SetupOffsetPayment() {
			if (LoanID == 0)
				return;

			var setupFee = model.Loan.Fees.FirstOrDefault(f => f.LoanFeeTypeID == (int)NLFeeTypes.SetupFee);

			Log.Debug("setup fee for offset: {0}", setupFee);

			if (setupFee != null) {
				NL_Payments setupfeeOffsetpayment = new NL_Payments {
					PaymentMethodID = (int)NLLoanTransactionMethods.SetupFeeOffset,
					Amount = setupFee.Amount,
					CreatedByUserID = 1,
					CreationTime = setupFee.CreatedTime,
					Notes = "setup fee offsetting",
					PaymentTime = nowTime.Date,
					PaymentStatusID = (int)NLPaymentStatuses.Active,
					LoanID = LoanID
				};
				AddPayment p = new AddPayment(model.CustomerID, setupfeeOffsetpayment, 1);
				p.Execute();
			}
		}

		//private void CopyRebateTransaction() {
		//	if (LoanID == 0)
		//		return;

		//	Migration.MigrateLoan.LoanTransactionModel rebateTransaction = DB.FillFirst<Migration.MigrateLoan.LoanTransactionModel>(
		//		"select t.PostDate,t.Amount,t.Description,t.IP,t.PaypointId,c.Id as CardID from LoanTransaction t " +
		//		"join PayPointCard c on c.TransactionId = t.PaypointId " +
		//		"where Description='system-repay' " +
		//		"and Status='Done' " +
		//		"and Type ='PaypointTransaction' " +
		//		"and LoanId = @loanID " +
		//		"and DateDiff(d, t.PostDate, @dd) = 0 " +
		//		 "and LoanTransactionMethodId = @methodId",
		//		CommandSpecies.Text, new QueryParameter("@loanID", model.Loan.OldLoanID), new QueryParameter("@dd", DateTime.UtcNow.Date), new QueryParameter("@methodId", (int)NLLoanTransactionMethods.Auto)
		//	 );

		//	if (rebateTransaction == null || rebateTransaction.Amount == 0) {
		//		Log.Debug("rebate transaction for oldLoanID {0} not found", model.Loan.OldLoanID);
		//		NL_AddLog(LogType.Info, string.Format("rebate transaction for oldLoanID {0} not found", model.Loan.OldLoanID), this.strategyArgs, null, Error, null);
		//		return;
		//	}

		//	NL_AddLog(LogType.Info, "Addloan:rebate", new object[] { rebateTransaction }, Error, null, null);

		//	// call AddPayment 
		//	NL_Payments rebatePayment = new NL_Payments() {
		//		Amount = rebateTransaction.Amount,
		//		CreatedByUserID = 1,
		//		LoanID = LoanID,
		//		PaymentStatusID = (int)NLPaymentStatuses.Active,
		//		PaymentMethodID = (int)NLLoanTransactionMethods.Auto,
		//		CreationTime = nowTime,
		//		PaymentTime = nowTime.AddMilliseconds(60), // workaround: guarantee that "setup offset" payment (with PaymentTime=nowTime) will be recorded before rebate //rebateTransaction.PostDate,
		//		Notes = "rebate"
		//	};
		//	rebatePayment.PaypointTransactions.Add(new NL_PaypointTransactions() {
		//		Amount = rebateTransaction.Amount,
		//		IP = rebateTransaction.IP,
		//		Notes = rebateTransaction.Description,
		//		PaypointTransactionStatusID = (int)NLPaypointTransactionStatuses.Done,
		//		PaypointUniqueID = rebateTransaction.PaypointId,
		//		PaypointCardID = rebateTransaction.CardID,
		//		TransactionTime = rebateTransaction.PostDate
		//	});

		//	AddPayment p = new AddPayment(model.CustomerID, rebatePayment, 1);
		//	p.Execute();
		//}

		private void SendMail(string subject, NL_LoanHistory history, List<NL_LoanFees> fees, List<NL_LoanSchedules> schedule, List<NL_LoanAgreements> agreements) {

			string emailToAddress = CurrentValues.Instance.Environment.Value.Contains("Dev") ? "elinar@ezbob.com" : CurrentValues.Instance.EzbobTechMailTo;
			string emailFromName = CurrentValues.Instance.MailSenderName;
			string emailFromAddress = CurrentValues.Instance.MailSenderEmail;

			string sMsg = string.Format("{0}. cust {1} user {2}, oldloan {3}, LoanID {4} error: {5}", subject, model.CustomerID, model.UserID, model.Loan.OldLoanID, LoanID, string.IsNullOrEmpty(Error) ? "No error" : Error);

			history.Schedule.Clear();
			history.Schedule = schedule;
			history.Agreements.Clear();
			history.Agreements = agreements;

			model.Loan.Histories.Clear();
			model.Loan.Histories.Add(history);
			model.Loan.Fees.Clear();
			model.Loan.Fees = fees;

			model.Loan.Payments.AddRange(DB.Fill<NL_Payments>("NL_PaymentsGet", CommandSpecies.StoredProcedure, new QueryParameter("@LoanID", LoanID)));
			if (model.Loan.Payments.Count > 0) {
				List<NL_LoanFeePayments> fps = DB.Fill<NL_LoanFeePayments>("NL_LoanFeePaymentsGet", CommandSpecies.StoredProcedure, new QueryParameter("@LoanID", LoanID));
				List<NL_LoanSchedulePayments> schp = DB.Fill<NL_LoanSchedulePayments>("NL_LoanSchedulePaymentsGet", CommandSpecies.StoredProcedure, new QueryParameter("@LoanID", LoanID));
				foreach (NL_Payments p in model.Loan.Payments) {
					p.SchedulePayments.AddRange(schp.Where(sp => sp.PaymentID == p.PaymentID).ToList());
					p.FeePayments.AddRange(fps.Where(fp => fp.PaymentID == p.PaymentID).ToList());
				}
			}

			var message = string.Format(
				"<h5>{0}</h5>"
					+ "<h5>Loan</h5> <pre>{1}</pre>"
					+ "<h5>FundTransfer</h5> <pre>{2}</pre>",
				HttpUtility.HtmlEncode(sMsg)
				, HttpUtility.HtmlEncode(model.Loan.ToString())
				, HttpUtility.HtmlEncode(model.FundTransfer == null ? "no FundTransfer specified" : model.FundTransfer.ToString()));

			new Mail().Send(
				emailToAddress,
				null,				// message text
				message,			//html
				emailFromAddress,	// fromEmail
				emailFromName,		// fromName
				subject
				);

		} // SendMail

	}
}//ns