namespace AutomationCalculator.ProcessHistory.AutoRejection
{
	using Newtonsoft.Json;

	public class MarketPlaceWithErrorException: ATrace
	{
		public MarketPlaceWithErrorException(int nCustomerID, DecisionStatus nDecisionStatus) : base(nCustomerID, nDecisionStatus) {}

		public MarketPlaceWithErrorExceptionModel Model { get; private set; }

		public void Init(MarketPlaceWithErrorExceptionModel model) {
			Model = model;
			Comment = string.Format("customer {0} {1} and consumer score {2} > {3} or business score {4} > {5}", CustomerID, model.HasMpError ? "Has error in MP" : "Don't have error in MP", model.MaxConsumerScore, model.MaxConsumerScoreThreshhold, model.MaxBusinessScore, model.MaxBusinessScoreThreshhold);
		} // Init

		public override string GetInitArgs() {
			return JsonConvert.SerializeObject(Model);
		}
	}

	public class MarketPlaceWithErrorExceptionModel {
		public bool HasMpError { get; set; }
		public int MaxConsumerScore { get; set; }
		public int MaxConsumerScoreThreshhold { get; set; }
		public int MaxBusinessScore { get; set; }
		public int MaxBusinessScoreThreshhold { get; set; }
	}
}
