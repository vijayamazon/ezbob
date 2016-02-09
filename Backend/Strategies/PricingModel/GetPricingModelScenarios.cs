namespace Ezbob.Backend.Strategies.PricingModel {
	using System.Collections.Generic;
	using Ezbob.Backend.Models;
	using Ezbob.Database;

	public class GetPricingModelScenarios : AStrategy {
		public GetPricingModelScenarios() {
			Scenarios = new List<PricingScenarioName>();
		} // constructor

		public override string Name { get { return "Get pricing scenarios names"; } }

		public List<PricingScenarioName> Scenarios { get; private set; }

		public override void Execute() {
			Scenarios = DB.Fill<PricingScenarioName>("GetPricingModelScenarios", CommandSpecies.StoredProcedure);
		} // Execute
	} // class GetPricingModelScenarios
} // namespace
