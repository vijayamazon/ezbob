namespace Ezbob.Backend.Strategies.Investor {
	using System;
	using Ezbob.Database;

	public class LinkOfferToInvestor : AStrategy {

		public LinkOfferToInvestor(int customerID, long cashRequestID, bool isForce, int? investorID, int? userID) {
			this.customerID = customerID;
			this.cashRequestID = cashRequestID;
			this.isForce = isForce;
			this.investorID = investorID;
			this.userID = userID;
			this.now = DateTime.UtcNow;
		}//ctor

		public override string Name { get { return "LinkOfferToInvestor"; } }

		public override void Execute() {
			IsForOpenPlatform = DB.ExecuteScalar<bool>("I_OfferForOpenPlatform", 
				CommandSpecies.StoredProcedure, 
				new QueryParameter("CashRequestID", this.cashRequestID));

			if (IsForOpenPlatform) {
				if (this.isForce && this.investorID.HasValue) {
					var forceInvestorForOffer = new ForceInvestorForOffer(this.customerID, this.cashRequestID, this.investorID.Value);
					forceInvestorForOffer.Execute();
					FoundInvestor = true;
				} else {
					var findInvestorForOffer = new FindInvestorForOffer(this.customerID, this.cashRequestID);
					findInvestorForOffer.Execute();
					FoundInvestor = findInvestorForOffer.IsFound;
				}

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
				int fundingBankAccountID = sr["InvestorBankAccountID"];
				decimal approvedSum = sr["ManagerApprovedSum"];
				decimal investmentPercent = sr["InvestmentPercent"];
				int currentInvestorID = sr["InvestorID"];
				const int negative = -1;

				AddInvestorSystemBalance addSystemBalance = new AddInvestorSystemBalance(fundingBankAccountID, 
					this.now, 
					approvedSum * investmentPercent * negative,
					null, 
					this.cashRequestID, 
					null, 
					null, 
					"Offer was approved",
					this.userID,
					this.now);
				addSystemBalance.Execute();

				var notifyInvestor = new NotifyInvestorUtilizedFunds(currentInvestorID);
				notifyInvestor.Execute();

			} catch (Exception ex) {
				Log.Warn(ex, "failed to add system balance for approved cash request {0} for investor {1}", 
					this.cashRequestID, sr["InvestorID"]);
			}
			return ActionResult.Continue;
		}//HandleOneAssignedToOfferInvestor

		public bool IsForOpenPlatform { get; private set; }
		public bool FoundInvestor { get; private set; }

		private readonly long cashRequestID;
		private readonly bool isForce;
		private readonly int? investorID;
		private readonly int? userID;
		private readonly int customerID;
		private readonly DateTime now;
	}//LinkOfferToInvestor
}//ns
