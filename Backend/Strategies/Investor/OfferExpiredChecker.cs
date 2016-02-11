namespace Ezbob.Backend.Strategies.Investor {
	using System;
	using DbConstants;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
	using Ezbob.Backend.Strategies.MailStrategies;
	using Ezbob.Backend.Strategies.MainStrategy.Helpers;
	using Ezbob.Backend.Strategies.SalesForce;
	using Ezbob.Database;
	using EZBob.DatabaseLib.Model.Database;
	using SalesForceLib.Models;

	public class OfferExpiredChecker : AStrategy {
		public OfferExpiredChecker() {
			this.now = DateTime.UtcNow;
		}//ctor

		public override string Name { get { return "OfferExpiredChecker"; } }

		public override void Execute() {

			DB.ForEachRowSafe(HandleOneExpired, "I_LoadExpiredOffers",
				CommandSpecies.StoredProcedure,
				new QueryParameter("Now", this.now)
				);
		}//Execute

		private ActionResult HandleOneExpired(SafeReader sr, bool bRowSetStart) {
			try {
				long cashRequestID = sr["CashRequestID"];
				int customerID = sr["CustomerID"];
				decimal approvedSum = sr["ManagerApprovedSum"];
				decimal creditSum = sr["CreditSum"];
				int? investorID = sr["InvestorID"];
				decimal investmentPercent = sr["InvestmentPercent"];
				int? fundingBankAccountID = sr["InvestorBankAccountID"];
				string customerEmail = sr["Email"];
				string decisionStr = sr["Decision"];
				CreditResultStatus result;
				if (!Enum.TryParse(decisionStr, out result)) {
					Log.Error("Failed parsing decision {0} for request {1} customer {2} ", decisionStr, cashRequestID, customerID);
					return ActionResult.Continue;
				}

				switch (result) {
					case CreditResultStatus.Approved:
						MarkOfferAsExpired(cashRequestID);
						UpdateSystemBalance(investorID, fundingBankAccountID, creditSum, investmentPercent, cashRequestID);
						break;
					case CreditResultStatus.PendingInvestor:
						MarkOfferAsExpired(cashRequestID);
						RejectCustomer(customerID, cashRequestID, customerEmail);
						break;
					default:
						Log.Error("Unsupported decision {0} for request {1} customer {2} ", decisionStr, cashRequestID, customerID);
						break;
				}
			} catch (Exception ex) {
				Log.Warn(ex, "Failed to add system balance raw when expired offer {0} for investor {1}", sr["CashRequestID"], sr["InvestorID"]);
			}//try
			return ActionResult.Continue;
		}//HandleOneExpired

		private void RejectCustomer(int customerID, long cashRequestID, string customerEmail) {
			MainStrategyUpdateCrC sp = new MainStrategyUpdateCrC(
				this.now,
				customerID,
				cashRequestID,
				new AutoDecisionResponse(customerID) {
					AutoRejectReason = "Pending investor offer expired",
					CreditResult = CreditResultStatus.Rejected,
					Decision = DecisionActions.Reject,
					SystemDecision = SystemDecision.Reject,
					UserStatus = Status.Rejected,
					DecisionName = "Rejection",
				},
				DB,
				Log
			);

			sp.ExecuteNonQuery();

			new UpdateOpportunity(customerID, new OpportunityModel {
				Email = customerEmail,
				DealCloseType = OpportunityDealCloseReason.Lost.ToString(),
				DealLostReason = "Auto Reject",
				CloseDate = DateTime.UtcNow,
			}).Execute();

			new RejectUser(customerID, true).Execute();
		}//RejectCustomer

		private void MarkOfferAsExpired(long cashRequestID) {
			DB.ExecuteNonQuery(@"UPDATE 
										CashRequests 
									 SET 
										IsExpired = 1 
									 WHERE 
										Id = " + cashRequestID,
					CommandSpecies.Text);
		}//MarkOfferAsExpired

		private int UpdateSystemBalance(int? investorID, int? fundingBankAccountID, decimal creditSum, decimal investmentPercent, long cashRequestID) {
			if (investorID.HasValue && fundingBankAccountID.HasValue) {
				var addInvestorSystemBalance = new AddInvestorSystemBalance(fundingBankAccountID.Value, this.now, creditSum * investmentPercent,
					null, cashRequestID, null, null, "Offer expired", 1, this.now);
				addInvestorSystemBalance.Execute();
				return addInvestorSystemBalance.SystemBalanceID;
			}
			return 0;
		}//UpdateSystemBalance

		private readonly DateTime now;
	}//OfferExpiredChecker
}//ns
