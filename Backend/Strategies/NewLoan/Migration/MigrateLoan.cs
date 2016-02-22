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

	public class MigrateLoans : AStrategy {

		public MigrateLoans() {
			nowTime = DateTime.UtcNow;
			this.strategyArgs = new object[] { nowTime };
			loanRep = ObjectFactory.GetInstance<LoanRepository>();
		}

		public override string Name { get { return "MigrateLoans"; } }
		public string Error { get; private set; }
		public long LoanID { get; private set; }
		public NL_Model model { get; private set; }

		private readonly object[] strategyArgs;
		public DateTime nowTime { get; private set; }

		public LoanRepository loanRep { get; private set; }

		public class CashReqModel {
			public long CashRequestID { get; set; }
			public long DecisionID { get; set; }
			public long OfferID { get; set; }
		}
	
		public class OldLoanIds {
			public int Id { get; set; }
		}

		public override void Execute() {

			// 1. copy CashRequest into NL_CashRequests; NL_decisions; NL_Offers; NL_OfferFees.

			NL_AddLog(LogType.Info, "Strategy start", this.strategyArgs, null, null, null);

			string query = "select top 20 Id from Loan l left join NL_Loans nl on nl.OldLoanID=l.Id and l.Id=null and l.Modified=0";

			List<OldLoanIds> loansList = DB.Fill<OldLoanIds>(query, CommandSpecies.Text);

			NL_AddLog(LogType.Info, "Get loans to migrate", query, loansList.Count, null, null);

			if (loansList.Count == 0) {
				Error = "Loans to migrate to NL not found";
				Log.Debug(Error);
				NL_AddLog(LogType.Info, "Strategy end", this.strategyArgs, null, Error, null);
				return;
			}

			//loansList.ForEach(ll => Log.Debug(ll.Id));
			//return;

			foreach (OldLoanIds l in loansList) {

				Log.Debug("Processing loan {0}", l.Id);

				Loan oldLoan = loanRep.Get(l.Id);

				if (oldLoan == null) {
					Error = string.Format("Failed to load oldID {0}", oldLoan);
					Log.Debug(Error);
					NL_AddLog(LogType.Info, Error, this.strategyArgs, null, Error, null);
					continue;
				}

				Context.CustomerID = oldLoan.Customer.Id;
				Context.UserID = oldLoan.CashRequest.IdUnderwriter;

				try {

					query = "select cr.CashRequestID, dd.lastDesicionID DecisionID, o.lastOffer OfferID from  NL_CashRequests cr left join " +
						"(select MAX([DecisionID]) as lastDesicionID, d.CashRequestID as CRID from [dbo].[NL_Decisions] d join [dbo].[NL_CashRequests] cr1 " +
						"on d.CashRequestID=cr1.CashRequestID and cr1.CustomerID=CustomerID group by d.CashRequestID) dd on dd.CRID=cr.CashRequestID " +
						"left join (select MAX(OfferID) as lastOffer, DecisionID from [dbo].[NL_Offers] group by DecisionID) o on o.DecisionID=dd.lastDesicionID " +
						"where [OldCashRequestID]=@crID";

					Log.Debug("Processing loan {0}, cr {1}, {2}", l.Id, oldLoan.CashRequest.Id, query);

					CashReqModel crModel = DB.FillFirst<CashReqModel>(query, CommandSpecies.Text, new QueryParameter("@crID", oldLoan.CashRequest.Id));

					NL_AddLog(LogType.Info, "NL crModel", new object[] { query, oldLoan.CashRequest.Id, l.Id }, crModel, null, null);

					// NL_CashRequests
					if (crModel.CashRequestID == 0L) {
						AddCashRequest sCashRequest = new AddCashRequest(new NL_CashRequests {
							CustomerID = oldLoan.Customer.Id,
							RequestTime = (DateTime)oldLoan.CashRequest.CreationDate,
							CashRequestOriginID = oldLoan.CashRequest.Originator.HasValue ? (int)oldLoan.CashRequest.Originator.Value : 5, // NL_CashRequestOrigins "Other"
							UserID = oldLoan.CashRequest.IdUnderwriter ?? 1,
							OldCashRequestID = oldLoan.CashRequest.Id
						});
						sCashRequest.Execute();
						sCashRequest.Context.CustomerID = Context.CustomerID;
						sCashRequest.Context.UserID = Context.UserID;
						crModel.CashRequestID = sCashRequest.CashRequestID;
					}

					if (crModel.CashRequestID == 0L) {
						Error = string.Format("Failed to add/find nl CR, oldID {0}, crID {1}", l.Id, oldLoan.CashRequest.Id);
						Log.Debug(Error);
						NL_AddLog(LogType.Info, Error, this.strategyArgs, null, Error, null);
						continue;
					}

					// copy decisions history
					foreach (DecisionHistory dh in oldLoan.CashRequest.DecisionHistories) {

						List<NL_DecisionRejectReasons> rejectReasons = new List<NL_DecisionRejectReasons>();
						if (dh.RejectReasons.Count > 0) {
							dh.RejectReasons.ForEach(x => rejectReasons.Add(new NL_DecisionRejectReasons() { RejectReasonID = x.RejectReason.Id }));
						}

						AddDecision sDesicion = new AddDecision(new NL_Decisions {
							CashRequestID = dh.CashRequest.Id,
							UserID = dh.Underwriter.Id,
							DecisionNameID = (int)NL_Model.DecisionToDecisionActions(Convert.ToString(dh.Action)),
							DecisionTime = dh.Date,
							Notes = string.Format("{0}. (migrated: old crID {1}, loan {2})", dh.Comment, oldLoan.CashRequest.Id, l.Id)
						}, oldLoan.CashRequest.Id, rejectReasons);
						
						sDesicion.Context.CustomerID = dh.Customer.Id;
						sDesicion.Context.UserID = dh.Underwriter.Id;

						sDesicion.Execute();

						crModel.DecisionID = sDesicion.DecisionID;

						// NL_Offers for approve decision
						if (dh.Action.Equals(DecisionActions.Approve)) {
							NL_OfferFees offerFee = new NL_OfferFees {
								LoanFeeTypeID = (int)NLFeeTypes.SetupFee,
								Percent = oldLoan.CashRequest.ManualSetupFeePercent ?? 0,
								OneTimePartPercent = 1,
								DistributedPartPercent = 0
							};

							if (oldLoan.CashRequest.SpreadSetupFee != null && oldLoan.CashRequest.SpreadSetupFee == true) {
								offerFee.LoanFeeTypeID = (int)NLFeeTypes.ServicingFee;
								offerFee.OneTimePartPercent = 0;
								offerFee.DistributedPartPercent = 1;
							}

							NL_OfferFees[] fees = { offerFee };

							AddOffer sOffer = new AddOffer(new NL_Offers {
								DecisionID = crModel.DecisionID,
								LoanTypeID = (int)NL_Model.LoanTypeNameToNLLoanType(oldLoan.CashRequest.LoanType.Name),
								LoanSourceID = oldLoan.CashRequest.LoanSource.ID,
								RepaymentIntervalTypeID = (int)RepaymentIntervalTypes.Month,
								Amount = oldLoan.CashRequest.ApprovedSum(),
								StartTime = (DateTime)oldLoan.CashRequest.OfferStart,
								EndTime = (DateTime)oldLoan.CashRequest.OfferValidUntil,
								CreatedTime = (DateTime)oldLoan.CashRequest.UnderwriterDecisionDate, // TODO ????
								DiscountPlanID = oldLoan.CashRequest.DiscountPlan.Id, // supposed discounts transformed right 
								MonthlyInterestRate = oldLoan.CashRequest.InterestRate,
								RepaymentCount = oldLoan.CashRequest.ApprovedRepaymentPeriod ?? oldLoan.CashRequest.RepaymentPeriod,
								BrokerSetupFeePercent = oldLoan.CashRequest.BrokerSetupFeePercent,
								IsLoanTypeSelectionAllowed = oldLoan.CashRequest.IsCustomerRepaymentPeriodSelectionAllowed,
								IsRepaymentPeriodSelectionAllowed = oldLoan.CashRequest.IsCustomerRepaymentPeriodSelectionAllowed,
								SendEmailNotification = !oldLoan.CashRequest.EmailSendingBanned,
								Notes = string.Format("{0}. (migrated: old crID {1}, loan {2})", oldLoan.CashRequest.UnderwriterComment, oldLoan.CashRequest.Id, l.Id),
								ProductSubTypeID = oldLoan.CashRequest.ProductSubTypeID ?? null
							}, fees);

							sDesicion.Context.CustomerID = dh.Customer.Id;
							sDesicion.Context.UserID = dh.Underwriter.Id;

							sOffer.Execute();

							crModel.OfferID = sOffer.OfferID;
						}
					}

					//ConnectionWrapper pconn = DB.GetPersistent();
					//try {
					//	pconn.BeginTransaction();
					//	pconn.Commit();
					//} catch (Exception ex) {
					//	pconn.Rollback();
					//	LoanID = 0;
					//	Error = ex.Message;
					//	Log.Error("Failed to add new loan: {0}", Error);
					//}

					// ReSharper disable once CatchAllClause
				} catch (Exception exc) {

					NL_AddLog(LogType.Error, "Strategy failed", this.strategyArgs, Error, exc.ToString(), exc.StackTrace);
					return;
				}
			}

		}//Execute

	}
}//ns