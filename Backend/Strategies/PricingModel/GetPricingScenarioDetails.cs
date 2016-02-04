namespace Ezbob.Backend.Strategies.PricingModel {
	using Ezbob.Database;

	public class GetPricingScenarioDetails : AStrategy {
		public GetPricingScenarioDetails(long scenarioID) {
			this.scenarioID = scenarioID;

			Model = new PricingModelModel();
		} // constructor

		public override string Name {
			get { return "Get pricing scenario details"; }
		} // Name

		public PricingModelModel Model { get; private set; }

		public override void Execute() {
			SafeReader sr = DB.GetFirst(
				"GetPricingScenarioDetails",
				CommandSpecies.StoredProcedure,
				new QueryParameter("ScenarioID", this.scenarioID)
			);

			if (!sr.IsEmpty)
				sr.Stuff(Model);
		} // Execute

		private readonly long scenarioID;
	} // class GetPricingScenarioDetails
} // namespace
