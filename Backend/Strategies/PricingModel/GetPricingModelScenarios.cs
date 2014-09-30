namespace EzBob.Backend.Strategies.PricingModel
{
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class GetPricingModelScenarios : AStrategy
	{
		public GetPricingModelScenarios(AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
			Scenarios = new List<string>();
		}

		public override string Name {
			get { return "Get pricing model scenarios"; }
		}

		public List<string> Scenarios { get; private set; }
		
		public override void Execute()
		{
			DB.ForEachRowSafe((sr, bRowsetStart) => {
				Scenarios.Add(sr["ScenarioName"]);
				return ActionResult.Continue;
			}, "GetPricingModelScenarios", CommandSpecies.StoredProcedure);
		}
	}
}
