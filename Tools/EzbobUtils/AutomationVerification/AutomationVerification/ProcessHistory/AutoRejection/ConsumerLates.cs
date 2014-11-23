namespace AutomationCalculator.ProcessHistory.AutoRejection
{
	public class ConsumerLates: ATrace
	{
		public ConsumerLates(DecisionStatus nDecisionStatus) : base(nDecisionStatus) { }

		public DataModel Model { get; private set; }

		public void Init(DataModel model) {
			Model = model;
			Comment = string.Format(
				"customer has {0} late accounts ({1} allowed) with {2} late days ({3} allowed)",
				model.NumOfLates, model.NumOfLatesThreshhold,
				model.LateDays, model.LateDaysThreshhold);
		} // Init

		public class DataModel {
			public int NumOfLates { get; set; }
			public int LateDays { get; set; }

			public int NumOfLatesThreshhold { get; set; }
			public int LateDaysThreshhold { get; set; }
		}
	}
}
