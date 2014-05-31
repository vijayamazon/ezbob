namespace EzBob.Backend.Strategies.PricingModel
{
	using System.Collections.Generic;
	using System.Data;
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
			DataTable dt = DB.ExecuteReader("GetPricingModelScenarios", CommandSpecies.StoredProcedure);
			foreach (DataRow row in dt.Rows)
			{
				var sr = new SafeReader(row);
				Scenarios.Add(sr["ScenarioName"]);
			}
		}
	}
}
