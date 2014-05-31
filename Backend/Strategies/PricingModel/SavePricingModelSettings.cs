﻿namespace EzBob.Backend.Strategies.PricingModel
{
	using System.Data;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class SavePricingModelSettings : AStrategy
	{
		private readonly string scenarioName;
		private readonly PricingModelModel model;

		public SavePricingModelSettings(string scenarioName, PricingModelModel model, AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
			this.scenarioName = scenarioName;
			this.model = model;
		}

		public override string Name {
			get { return "Save pricing model configs"; }
		}
		
		public override void Execute()
		{
			DB.ExecuteNonQuery(
				"SavePricingModelConfigsForScenario",
				CommandSpecies.StoredProcedure,
				new QueryParameter("ScenarioName", scenarioName),
				new QueryParameter("TenurePercents", model.TenurePercents),
				new QueryParameter("SetupFee", model.SetupFeePercents),
				new QueryParameter("ProfitMarkupPercentsOfRevenue", model.ProfitMarkup),
				new QueryParameter("OpexAndCapex", model.OpexAndCapex),
				new QueryParameter("InterestOnlyPeriod", model.InterestOnlyPeriod),
				new QueryParameter("EuCollectionRate", model.EuCollectionRate),
				new QueryParameter("DefaultRateCompanyShare", model.DefaultRateCompanyShare),
				new QueryParameter("DebtPercentOfCapital", model.DebtPercentOfCapital),
				new QueryParameter("CostOfDebtPA", model.CostOfDebt),
				new QueryParameter("CollectionRate", model.CollectionRate),
				new QueryParameter("Cogs", model.Cogs),
				new QueryParameter("BrokerSetupFee", model.BrokerSetupFeePercents)
			);
		}
	}
}
