namespace Ezbob.Backend.Strategies.Investor {
	using Ezbob.Backend.ModelsWithDB.OpenPlatform;
	using Ezbob.Database;

	public class ForceInvestorForOffer : AStrategy {
		public ForceInvestorForOffer(int customerID, long cashRequestID, int investorID) {
			this.customerID = customerID;
			this.cashRequestID = cashRequestID;
			this.investorID = investorID;
		}//ctor

		public override string Name { get { return "ForceInvestorForOffer"; } }

		public override void Execute() {
			DB.ExecuteNonQuery("I_OpenPlatformOfferSave", CommandSpecies.StoredProcedure,
				DB.CreateTableParameter("Tbl", new I_OpenPlatformOffer {
					CashRequestID = this.cashRequestID,
					InvestorID = this.investorID,
					InvestmentPercent = 1.0M //100% in force investment percent
				})
			);
		}//Execute

		private readonly int customerID;
		private readonly long cashRequestID;
		private readonly int investorID;
	}//ForceInvestorForOffer
}//ns
