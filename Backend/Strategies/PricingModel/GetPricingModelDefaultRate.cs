namespace Ezbob.Backend.Strategies.PricingModel {
	using Ezbob.Database;

	public class GetPricingModelDefaultRate : AStrategy {
		public decimal DefaultRate { get; private set; }
		public int ConsumerScore { get; private set; }
		public int BusinessScore { get; private set; }

		public GetPricingModelDefaultRate(int customerId, decimal companyShare) {
			this.companyShare = companyShare;
			this.customerId = customerId;
		} // constructor

		public override string Name { get { return "Get pricing model default rate"; } }

		public override void Execute() {
			decimal customerShare = 1 - this.companyShare;

			var sr = DB.GetFirst(
				"GetOfferConsumerBusinessDefaultRates",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", this.customerId)
			);

			decimal companyValue = sr["BusinessDefaultRate"];
			decimal customerValue = sr["ConsumerDefaultRate"];
			ConsumerScore = sr["ConsumerScore"];
			BusinessScore = sr["BusinessScore"];
			DefaultRate = this.companyShare * companyValue + customerShare * customerValue;

			Log.Info(
				"GetPricingModelDefaultRate company value {0}, company share {1}, customer value {2}, " +
				"customer share {3}, default rate {4} consumer score {5} business score {6}",
				companyValue,
				this.companyShare,
				customerValue,
				customerShare,
				DefaultRate,
				ConsumerScore,
				BusinessScore
			);
		} // Execute

		private readonly decimal companyShare;
		private readonly int customerId;
	} // class GetPricingModelDefaultRate
} // namespace
