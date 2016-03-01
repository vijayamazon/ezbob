namespace Ezbob.Backend.Strategies.NewLoan.Migration {
	using System;
	using System.Collections.Generic;
	using DbConstants;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Database;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using NHibernate.Linq;
	using StructureMap;

	/// <summary>
	/// copy CashRequest, DecisionsHistory into NL_CashRequests NL_decisions NL_Offers NL_OfferFees NL_decisionReasons
	/// </summary>
	public class MigrateCRDecisionOffer : AStrategy {
		public MigrateCRDecisionOffer() {
			loanRep = ObjectFactory.GetInstance<LoanRepository>();
		}

		public override string Name { get { return "MigrateCRDecisionOffer"; } }
		public string Error { get; private set; }
		public LoanRepository loanRep { get; private set; }

		public override void Execute() {

			List<MigrateLoanTransaction.LoanId> loansList = DB.Fill<MigrateLoanTransaction.LoanId>("NL_MigrationCRDecisionOffer", CommandSpecies.StoredProcedure);

			NL_AddLog(LogType.Info, "Strategy start", "NL_MigrationCRDecisionOffer", null, null, null);

			if (loansList.Count == 0) {
				Error = "Loans to migrate to NL not found";
				Log.Debug(Error);
				NL_AddLog(LogType.Info, "Strategy end", null, null, Error, null);
				return;
			}

			NL_AddLog(LogType.Info, "Loans to migrate", null, loansList.Count, null, null);

			string query;
 
			foreach (MigrateLoanTransaction.LoanId l in loansList) {

				Loan loan = loanRep.Get(l.Id);

				if (loan == null) {
					Error = string.Format("Failed to load old loan {0}", l.Id);
					Log.Info(Error);
					NL_AddLog(LogType.Info, Error, l.Id, null, Error, null);
					continue;
				}

				Context.CustomerID = loan.Customer.Id;
				Context.UserID = loan.CashRequest.IdUnderwriter;
	
				try {

					// [ELINAR-PC].[ezbob].[dbo].[NL_CashRequestGetByOldID]

					query="select CashRequestID from NL_CashRequests where [OldCashRequestID]=" + loan.CashRequest.Id; //= "select top 20 Id from Loan l left join NL_Loans nl on nl.OldLoanID=l.Id and l.Id=null and l.Modified=0";

					Log.Debug("Processing loan {0}, cr {1}, {2}", loan.Id, loan.CashRequest.Id, query);

					MigrateLoanTransaction.CashReqModel crModel = DB.FillFirst<MigrateLoanTransaction.CashReqModel>(query, CommandSpecies.Text, new QueryParameter("@crID", loan.CashRequest.Id));

					NL_AddLog(LogType.Info, "crModel", new object[] { query, loan.CashRequest.Id, l.Id }, crModel, null, null);

					// copy to NL_CashRequests
					if (crModel.CashRequestID == 0L) {
						AddCashRequest sCashRequest = new AddCashRequest(new NL_CashRequests {
							CustomerID = loan.Customer.Id,
							RequestTime = (DateTime)loan.CashRequest.CreationDate,
							CashRequestOriginID = loan.CashRequest.Originator.HasValue ? (int)loan.CashRequest.Originator.Value : 5, // NL_CashRequestOrigins "Other"
							UserID = loan.CashRequest.IdUnderwriter ?? 1,
							OldCashRequestID = loan.CashRequest.Id
						});
						
						sCashRequest.Context.CustomerID = Context.CustomerID;
						sCashRequest.Context.UserID = Context.UserID;

						sCashRequest.Execute();
						crModel.CashRequestID = sCashRequest.CashRequestID;
					}

					if (crModel.CashRequestID == 0L) {
						Error = string.Format("Failed to add/find nl CR, oldID {0}, crID {1}", loan.Id, loan.CashRequest.Id);
						Log.Info(Error);
						NL_AddLog(LogType.Info, "NL CR failed", loan.CashRequest.Id, null, Error, null);
						continue;
					}

					// copy decisions
					foreach (DecisionHistory dh in loan.CashRequest.DecisionHistories) {

						query = string.Format("select DecisionID from [dbo].[NL_Decisions] d join [dbo].[NL_CashRequests] c on d.CashRequestID=c.CashRequestID and c.OldCashRequestID={0} and d.UserID={1} " +
							"and d.DecisionNameID={2} and d.DecisionTime='{3}'",
								dh.CashRequest.Id, dh.Underwriter.Id, (int)dh.Action, dh.Date.ToString("yyyy-MM-dd HH:mm:ss"));

						//query = "select DecisionID from [dbo].[NL_Decisions] d join [dbo].[NL_CashRequests] c on d.CashRequestID=c.CashRequestID and c.OldCashRequestID=@crID " +
						//	"and d.UserID=@uID and d.DecisionNameID=@dID and d.DecisionTime='@dDate'";
							//"datediff(DAY, d.DecisionTime, '@dDate')=0";

						MigrateLoanTransaction.CashReqModel dModel = DB.FillFirst<MigrateLoanTransaction.CashReqModel>(query, CommandSpecies.Text/*, 
							new QueryParameter("@crID", dh.CashRequest.Id),
							new QueryParameter("@uID", dh.Underwriter.Id),
							new QueryParameter("@dID", (int)dh.Action),
							new QueryParameter("@dDate", dh.Date.ToString("yyyy-MM-dd HH:mm:ss"))*/); 

						NL_AddLog(LogType.Info, "dModel", new object[] { query, dh.CashRequest.Id, dh.Underwriter.Id, (int)dh.Action, dh.Date }, dModel, null, null);

						if (dModel.DecisionID == 0L) {

							List<NL_DecisionRejectReasons> rejectReasons = new List<NL_DecisionRejectReasons>();
							if (dh.RejectReasons.Count > 0) {
								dh.RejectReasons.ForEach(x => rejectReasons.Add(new NL_DecisionRejectReasons() {
									RejectReasonID = x.RejectReason.Id
								}));
							}

							AddDecision sDesicion = new AddDecision(new NL_Decisions {
								CashRequestID = dh.CashRequest.Id,
								UserID = dh.Underwriter.Id,
								DecisionNameID = (int)dh.Action,
								DecisionTime = dh.Date,
								Notes = string.Format("{0}. (migrated: old crID {1}, loan {2})", dh.Comment, loan.CashRequest.Id, l.Id)
							}, loan.CashRequest.Id, rejectReasons);

							sDesicion.Context.CustomerID = dh.Customer.Id;
							sDesicion.Context.UserID = dh.Underwriter.Id;

							sDesicion.Execute();
							dModel.DecisionID = sDesicion.DecisionID;
							crModel.DecisionID = dModel.DecisionID;
						}

						// NL_Offers for approve decision
						if (dh.Action.Equals(DecisionActions.Approve)) {

							query = string.Format("select OfferID from NL_Offers where DecisionID={0}", dModel.DecisionID);

							dModel = DB.FillFirst<MigrateLoanTransaction.CashReqModel>(query, CommandSpecies.Text);

							NL_AddLog(LogType.Info, "dModel-OfferID", new object[] { query }, dModel, null, null);

							if (dModel.OfferID == 0L) {

								NL_OfferFees offerFee = new NL_OfferFees {
									LoanFeeTypeID = (int)NLFeeTypes.SetupFee,
									Percent = loan.CashRequest.ManualSetupFeePercent ?? 0,
									OneTimePartPercent = 1,
									DistributedPartPercent = 0
								};

								if (loan.CashRequest.SpreadSetupFee != null && loan.CashRequest.SpreadSetupFee == true) {
									offerFee.LoanFeeTypeID = (int)NLFeeTypes.ServicingFee;
									offerFee.OneTimePartPercent = 0;
									offerFee.DistributedPartPercent = 1;
								}

								NL_OfferFees[] fees = {
									offerFee
								};

								AddOffer sOffer = new AddOffer(new NL_Offers {
									DecisionID = dModel.DecisionID,
									LoanTypeID = (int)NL_Model.LoanTypeNameToNLLoanType(loan.CashRequest.LoanType.Name),
									LoanSourceID = loan.CashRequest.LoanSource.ID,
									RepaymentIntervalTypeID = (int)RepaymentIntervalTypes.Month,
									Amount = loan.CashRequest.ApprovedSum(),
									StartTime = (DateTime)loan.CashRequest.OfferStart,
									EndTime = (DateTime)loan.CashRequest.OfferValidUntil,
									CreatedTime = (DateTime)loan.CashRequest.UnderwriterDecisionDate, // TODO ????
									DiscountPlanID = loan.CashRequest.DiscountPlan.Id, // supposed discounts transformed right 
									MonthlyInterestRate = loan.CashRequest.InterestRate,
									RepaymentCount = loan.CashRequest.ApprovedRepaymentPeriod ?? loan.CashRequest.RepaymentPeriod,
									BrokerSetupFeePercent = loan.CashRequest.BrokerSetupFeePercent,
									IsLoanTypeSelectionAllowed = loan.CashRequest.IsCustomerRepaymentPeriodSelectionAllowed,
									IsRepaymentPeriodSelectionAllowed = loan.CashRequest.IsCustomerRepaymentPeriodSelectionAllowed,
									SendEmailNotification = !loan.CashRequest.EmailSendingBanned,
									Notes = string.Format("{0}. (migrated: old crID {1}, loan {2})", loan.CashRequest.UnderwriterComment, loan.CashRequest.Id, l.Id),
									ProductSubTypeID = loan.CashRequest.ProductSubTypeID ?? null
								}, fees);

								sOffer.Context.CustomerID = dh.Customer.Id;
								sOffer.Context.UserID = dh.Underwriter.Id;

								sOffer.Execute();
								crModel.OfferID = sOffer.OfferID;
							}
						}
					}

					// do in backfill -  after migration end
					// update .[dbo].[MedalCalculations], .[dbo].[MedalCalculationsAV]
				//	query =
				//		"IF (SELECT [NLCashRequestID] FROM [dbo].[MedalCalculations] WHERE [CashRequestID]=@crID) is null begin update [dbo].[MedalCalculations] set [NLCashRequestID]=@nlcrID where [CashRequestID]=@crID end " +
				//			"IF (SELECT [NLCashRequestID] FROM [dbo].[MedalCalculationsAV] WHERE [CashRequestID]=@crID) is null begin update [dbo].[MedalCalculationsAV] set [NLCashRequestID]=@nlcrID where [CashRequestID]=@crID end";
				//	+	"IF (SELECT [NLOfferID] FROM [dbo].[I_InvestorSystemBalance] WHERE [CashRequestID]=@crID) is null begin update [dbo].[I_InvestorSystemBalance] set [NLOfferID]=@nlcrID where [CashRequestID]=@crID end ";				
					
				//	DB.ExecuteNonQuery(query, CommandSpecies.Text, new QueryParameter("@crID", loan.CashRequest.Id), new QueryParameter("@nlcrID", crModel.CashRequestID));

					NL_AddLog(LogType.Info, "Strategy end", loansList, Error, null, null);

					// ReSharper disable once CatchAllClause
				} catch (Exception exc) {
					Error = exc.Message;
					Log.Alert(exc);
					NL_AddLog(LogType.Error, "Strategy failed", null, Error, exc.ToString(), exc.StackTrace);
					return;
				}
			}
		}
	}
}//ns