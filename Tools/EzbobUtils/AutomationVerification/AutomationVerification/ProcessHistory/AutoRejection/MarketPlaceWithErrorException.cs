namespace AutomationCalculator.ProcessHistory.AutoRejection
{
	public class MarketPlaceWithErrorException: ATrace
	{
		public MarketPlaceWithErrorException(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {}

		public DataModel Model { get; private set; }

		public void Init(DataModel model) {
			Model = model;
			Comment = string.Format(
				"customer has {0} error in MP and consumer score {1} > {2} or business score {3} > {4}",
				model.HasMpError ? "an" : "no",
				model.MaxConsumerScore,
				model.MaxConsumerScoreThreshhold,
				model.MaxBusinessScore,
				model.MaxBusinessScoreThreshhold
			);
		} // Init

		public class DataModel {
			public bool HasMpError { get; set; }
			public int MaxConsumerScore { get; set; }
			public int MaxConsumerScoreThreshhold { get; set; }
			public int MaxBusinessScore { get; set; }
			public int MaxBusinessScoreThreshhold { get; set; }
		}
	}
}
