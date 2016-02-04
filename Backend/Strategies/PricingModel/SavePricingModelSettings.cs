namespace Ezbob.Backend.Strategies.PricingModel {
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
				new QueryParameter("ScenarioID", scenarioID),
				new QueryParameter("TenurePercents", model.TenurePercents),
				new QueryParameter("SetupFee", model.SetupFeePercents),
				new QueryParameter("ProfitMarkupPercentsOfRevenue", model.ProfitMarkup),
				new QueryParameter("OpexAndCapex", model.OpexAndCapex),
				new QueryParameter("InterestOnlyPeriod", model.InterestOnlyPeriod),
				new QueryParameter("EuCollectionRate", model.EuCollectionRate),
				new QueryParameter("COSMECollectionRate", model.CosmeCollectionRate),
				new QueryParameter("DefaultRateCompanyShare", model.DefaultRateCompanyShare),
				new QueryParameter("DebtPercentOfCapital", model.DebtPercentOfCapital),
				new QueryParameter("CostOfDebtPA", model.CostOfDebt),
				new QueryParameter("CollectionRate", model.CollectionRate),
				new QueryParameter("Cogs", model.Cogs),
				new QueryParameter("BrokerSetupFee", model.BrokerSetupFeePercents)
			);
		} // Execute

		private readonly long scenarioID;
		private readonly PricingModelModel model;
	} // class SavePricingModelSettings
} // namespace
