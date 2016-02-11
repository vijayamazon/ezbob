namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using System.Collections.Generic;
	using DbConstants;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Database;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
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
			public long DesicionID { get; set; }
			public long offerID { get; set; }
		}

		/// <exception cref="InvalidOperationException">The <see cref="P:System.Nullable`1.HasValue" /> property is false.</exception>
		public override void Execute() {

			NL_AddLog(LogType.Info, "Strategy start", this.strategyArgs, null, null, null);

			List<int> loansList = DB.Fill<int>("select Id from Loan l left join NL_Loans nl on nl.OldLoanID=l.Id and l.Id=null and l.Modified=0",CommandSpecies.Text);

			if (loansList.Count==0) {
				Error = "Loans to migrate to NL not found";
				Log.Debug(Error);
				NL_AddLog(LogType.Info, "Strategy end", this.strategyArgs, null, Error, null);
				return;
			}

			foreach (int oldLoanID in loansList) {

				Loan oldLoan = loanRep.Get(oldLoanID);
				if (oldLoan == null) {
					Error = string.Format("Failed to load oldID {0}", oldLoan);
					Log.Debug(Error);
					NL_AddLog(LogType.Info, Error, this.strategyArgs, null, Error, null);
					continue;
				}

				try {
					CashReqModel crModel = DB.FillFirst<CashReqModel>("select cr.CashRequestID, dd.lastDesicionID DecisionID, o.lastOffer OfferID " +
						"from  NL_CashRequests cr join " +
						"(select MAX([DecisionID]) as lastDesicionID, d.CashRequestID as CRID from [dbo].[NL_Decisions] d join [dbo].[NL_CashRequests] cr1 " +
						"on d.CashRequestID=cr1.CashRequestID and cr1.CustomerID=CustomerID group by d.CashRequestID) dd on dd.CRID=cr.CashRequestID " +
						"left join (select MAX(OfferID) as lastOffer, DecisionID from [dbo].[NL_Offers] group by DecisionID) o on o.DecisionID=dd.lastDesicionID " +
						"where [OldCashRequestID]=@crID", CommandSpecies.Text, new QueryParameter("crID", oldLoan.CashRequest.Id));

					// NL_CashRequests
					if (crModel.CashRequestID == 0L) {
						AddCashRequest sCashRequest = new AddCashRequest(new NL_CashRequests() {
							CustomerID = oldLoan.Customer.Id,
							RequestTime = oldLoan.CashRequest.CreationDate ?? nowTime,
							CashRequestOriginID = oldLoan.CashRequest.Originator.HasValue ? (int)oldLoan.CashRequest.Originator.Value : 5, // NL_CashRequestOrigins "Other"
							UserID = oldLoan.CashRequest.IdUnderwriter ?? 1,
							OldCashRequestID = oldLoan.CashRequest.Id
						});
						sCashRequest.Execute();
						crModel.CashRequestID = sCashRequest.CashRequestID;
					}
					 
					if (crModel.CashRequestID == 0L) {
						Error = string.Format("Failed to add/find nl CR, oldID {0}, crID {1}", oldLoanID, oldLoan.CashRequest.Id);
						Log.Debug(Error);
						NL_AddLog(LogType.Info, Error, this.strategyArgs, null, Error, null);
						continue;
					}

					// NL_Decisions
					if (crModel.DesicionID == 0L) {
						AddDecision sDesicion = new AddDecision(new NL_Decisions() {
							CashRequestID = crModel.CashRequestID,
							UserID = oldLoan.CashRequest.IdUnderwriter ?? 1,
							DecisionNameID = (int)(oldLoan.CashRequest.UnderwriterDecision ?? CreditResultStatus.Approved),
							DecisionTime = oldLoan.CashRequest.UnderwriterDecisionDate ?? nowTime,
							Notes = string.Format("migrated from old crID {0}. {1}", oldLoanID, oldLoan.CashRequest.UnderwriterComment)
						}, oldLoan.CashRequest.Id, null);

						sDesicion.Execute();
						crModel.DesicionID = sDesicion.DecisionID;
					}

					if (crModel.DesicionID == 0L) {
						Error = string.Format("Failed to add/find nl DesicionID, old loanID {0}, old crID {1}", oldLoanID, oldLoanID);
						Log.Debug(Error);
						NL_AddLog(LogType.Info, Error, this.strategyArgs, null, Error, null);
						continue;
					}
				
					// NL_Offers
					if (crModel.offerID == 0L) {
						NL_Offers offer = new NL_Offers() {
							DecisionID = crModel.DesicionID,
							//LoanTypeID = NLLoanTypes.  oldLoan.CashRequest.LoanType.Name,
							Amount = (decimal)oldLoan.CashRequest.ManagerApprovedSum,
							StartTime = oldLoan.CashRequest.OfferStart.HasValue ?  oldLoan.CashRequest.OfferStart.Value: nowTime,
							EndTime = oldLoan.CashRequest.OfferValidUntil.HasValue?oldLoan.CashRequest.OfferValidUntil.Value:nowTime,
							CreatedTime = oldLoan.CashRequest.UnderwriterDecisionDate??nowTime, 
							//DiscountPlanID = oldLoan.CashRequest.DiscountPlan.Id, // todo convert to 
							LoanSourceID = oldLoan.CashRequest.LoanSource.ID, // (int)NLLoanSources.COSME,
							RepaymentIntervalTypeID = (int)RepaymentIntervalTypes.Month,
							MonthlyInterestRate = oldLoan.CashRequest.InterestRate,
							RepaymentCount = oldLoan.CashRequest.RepaymentPeriod,
							BrokerSetupFeePercent = oldLoan.CashRequest.BrokerSetupFeePercent,
							IsLoanTypeSelectionAllowed = oldLoan.CashRequest.IsCustomerRepaymentPeriodSelectionAllowed,
							IsRepaymentPeriodSelectionAllowed = oldLoan.CashRequest.IsCustomerRepaymentPeriodSelectionAllowed,
							SendEmailNotification = !oldLoan.CashRequest.EmailSendingBanned,
							Notes = "old crID: " + oldLoan.CashRequest.Id + ". " + oldLoan.CashRequest.UnderwriterComment
						};
						List<NL_OfferFees> offees = new List<NL_OfferFees>();
						//AddOffer sOffer = new AddOffer(offer, offees.Add(new NL_OfferFees() {

						//});
				}

				ConnectionWrapper pconn = DB.GetPersistent();
					try {
						pconn.BeginTransaction();


						pconn.Commit();
					} catch (Exception ex) {
						pconn.Rollback();

						LoanID = 0;
						Error = ex.Message;
						Log.Error("Failed to add new loan: {0}", Error);
					}
					
					

					// ReSharper disable once CatchAllClause
				} catch (Exception exc) {

					

					
		
					//NL_AddLog(LogType.Error, "Strategy Failed - Failed to add new loan", this.strategyArgs, Error, ex.ToString(), ex.StackTrace);
					return;
				}
			}
				
		}//Execute

	}
}//ns