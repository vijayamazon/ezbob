namespace Ezbob.Backend.Strategies.Investor {
	using System;
	using Ezbob.Database;

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
				decimal? investmentPercent = sr["InvestmentPercent"];
				int? fundingBankAccountID = sr["InvestorBankAccountID"];

				DB.ExecuteNonQuery(@"UPDATE 
										CashRequests 
									 SET 
										IsExpired = 1 
									 WHERE 
										Id = " + cashRequestID,
					CommandSpecies.Text);

				if (investorID.HasValue && fundingBankAccountID.HasValue) {
					var systemBalanceID = DB.ExecuteScalar<int>("I_SystemBalanceAdd",
						CommandSpecies.StoredProcedure,
						new QueryParameter("BankAccountID", fundingBankAccountID),
						new QueryParameter("Now", this.now),
						new QueryParameter("TransactionAmount", creditSum * investmentPercent),
						new QueryParameter("ServicingFeeAmount", null),
						new QueryParameter("LoanTransactionID", null));
				}
			} catch (Exception ex) {
				Log.Warn(ex, "Failed to add system balance raw when expired offer {0} for investor {1}", sr["CashRequestID"], sr["InvestorID"]);
			}
			return ActionResult.Continue;
		}//HandleOneExpired

		private readonly DateTime now;
	}//OfferExpiredChecker
}//ns
