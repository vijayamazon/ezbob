namespace Ezbob.Backend.Strategies.PricingModel {
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Database;

	public class SavePricingModelSettings : AStrategy {
		public SavePricingModelSettings(long scenarioID, PricingModelModel model) {
			this.scenarioID = scenarioID;
			this.model = model;
		} // constructor

		public override string Name { get { return "Save pricing model configs"; } }

		public override void Execute() {
			DB.ExecuteNonQuery(
				"SavePricingModelConfigsForScenario",
				CommandSpecies.StoredProcedure,
				new QueryParameter("ScenarioID", this.scenarioID),
				new QueryParameter("TenurePercents", this.model.TenurePercents),
				new QueryParameter("SetupFee", this.model.SetupFeePercents),
				new QueryParameter("ProfitMarkupPercentsOfRevenue", this.model.ProfitMarkup),
				new QueryParameter("OpexAndCapex", this.model.OpexAndCapex),
				new QueryParameter("InterestOnlyPeriod", this.model.InterestOnlyPeriod),
				new QueryParameter("EuCollectionRate", this.model.EuCollectionRate),
				new QueryParameter("COSMECollectionRate", this.model.CosmeCollectionRate),
				new QueryParameter("DefaultRateCompanyShare", this.model.DefaultRateCompanyShare),
				new QueryParameter("DebtPercentOfCapital", this.model.DebtPercentOfCapital),
				new QueryParameter("CostOfDebtPA", this.model.CostOfDebt),
				new QueryParameter("CollectionRate", this.model.CollectionRate),
				new QueryParameter("Cogs", this.model.Cogs),
				new QueryParameter("BrokerSetupFee", this.model.BrokerSetupFeePercents)
			);
		} // Execute

		private readonly long scenarioID;
		private readonly PricingModelModel model;
	} // class SavePricingModelSettings
} // namespace
