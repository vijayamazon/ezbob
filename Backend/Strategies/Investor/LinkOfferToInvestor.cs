namespace Ezbob.Backend.Strategies.Investor {
	using System;
	using Ezbob.Database;

	public class LinkOfferToInvestor : AStrategy {

		public LinkOfferToInvestor(int customerID, long cashRequestID) {
			this.customerID = customerID;
			this.cashRequestID = cashRequestID;
			this.now = DateTime.UtcNow;
		}//ctor

		public override string Name { get { return "LinkOfferToInvestor"; } }

		public override void Execute() {
			IsForOpenPlatform = DB.ExecuteScalar<bool>("I_OfferForOpenPlatform", 
				CommandSpecies.StoredProcedure, 
				new QueryParameter("CashRequestID", this.cashRequestID));

			if (IsForOpenPlatform) {
				var findInvestorForOffer = new FindInvestorForOffer(this.customerID, this.cashRequestID);
				findInvestorForOffer.Execute();
				FoundInvestor = findInvestorForOffer.IsFound;
				if (FoundInvestor) {
					DB.ForEachRowSafe(HandleOneAssignedToOfferInvestor, 
						"I_LoadAssigedToOfferInvestors", 
						CommandSpecies.StoredProcedure, 
						new QueryParameter("CashRequestID", this.cashRequestID));
				}
			}
		}//Execute

		private ActionResult HandleOneAssignedToOfferInvestor(SafeReader sr, bool bRowSetStart) {
			try {
				int fundingBankAccountID = sr["BankAccountID"];
				decimal approvedSum = sr["ManagerApprovedSum"];
				decimal investmentPercent = sr["InvestmentPercent"];
				int investorID = sr["InvestorID"];
				const int negative = -1;

				var systemBalanceID = DB.ExecuteScalar<int>("I_SystemBalanceAdd",
					CommandSpecies.StoredProcedure,
					new QueryParameter("BankAccountID", fundingBankAccountID),
					new QueryParameter("Now", this.now),
					new QueryParameter("TransactionAmount", approvedSum * investmentPercent * negative),
					new QueryParameter("ServicingFeeAmount", null),
					new QueryParameter("LoanTransactionID", null));

			} catch (Exception ex) {
				Log.Warn(ex, "failed to add system balance for approved cash request {0} for investor {1}", 
					this.cashRequestID, sr["InvestorID"]);
			}
			return ActionResult.Continue;
		}//HandleOneAssignedToOfferInvestor
		public bool IsForOpenPlatform { get; private set; }
		public bool FoundInvestor { get; private set; }

		private readonly long cashRequestID;
		private readonly int customerID;
		private readonly DateTime now;
	}//LinkOfferToInvestor
}//ns
