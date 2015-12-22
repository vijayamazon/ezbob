namespace Ezbob.Backend.Strategies.Investor {
	using Ezbob.Backend.ModelsWithDB.OpenPlatform;
	using Ezbob.Database;

	public class FindInvestorForOffer : AStrategy {
		public FindInvestorForOffer(int customerID, int cashRequestID) {
			this.customerID = customerID;
			this.cashRequestID = cashRequestID;
		}
		public override string Name { get { return "FindInvestorForOffer"; } }

		public override void Execute() {
			//todo implement find investor logic
			//todo for alpha only one investor should be found and he is full funded (100%) of offer
			//todo for alpha the logic for choosing one investor: was the last who assigned for a loan

			var foundInvetsorID = DB.ExecuteScalar<int?>("SELECT TOP 1 InvestorID FROM I_Investor", CommandSpecies.Text);
			if(!foundInvetsorID.HasValue) {
				IsFound = false;
				Log.Warn("No investors found in the system customer id: {0} cash request id: {1}", this.customerID, this.cashRequestID);
				return;
			}

			DB.ExecuteNonQuery("I_OpenPlatformOfferSave", CommandSpecies.StoredProcedure,
 				DB.CreateTableParameter("Tbl",
					new I_OpenPlatformOffer {
						CashRequestID = this.cashRequestID,
						InvestorID = foundInvetsorID.Value,
						InvestmentPercent = 1M //100% always in alpha
					})
				);

			IsFound = true;
		}

		public bool IsFound { get; private set; }

		private readonly int customerID;
		private readonly int cashRequestID;
	}
}
