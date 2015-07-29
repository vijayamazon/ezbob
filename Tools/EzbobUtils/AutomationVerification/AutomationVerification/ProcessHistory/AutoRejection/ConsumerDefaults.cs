namespace AutomationCalculator.ProcessHistory.AutoRejection {
	public class ConsumerDefaults : ATrace {
		public ConsumerDefaults(DecisionStatus nDecisionStatus) : base(nDecisionStatus) { }

		public DataModel Model { get; private set; }

		public void Init(DataModel model) {
			Model = model;
			Comment = string.Format(
				"customer has {0} default accounts ({1} allowed) with default amount of {2} (allowed {3}) and consumer score of {4} (allowed {5})",
				model.NumOfDefaults, model.NumDefaultAccountsThreshhold,
				model.AmountOfDefaults, model.AmountDefaultAccountsThreshhold,
				model.MaxConsumerScore, model.MaxConsumerScoreThreshhold);
		} // Init

		public class DataModel {
			public int MaxConsumerScore { get; set; }
			public int NumOfDefaults { get; set; }
			public int AmountOfDefaults { get; set; }

			public int MaxConsumerScoreThreshhold { get; set; }
			public int NumDefaultAccountsThreshhold { get; set; }
			public int AmountDefaultAccountsThreshhold { get; set; }

			public bool TooManyDefaults {
				get { return (NumOfDefaults >= NumDefaultAccountsThreshhold); } // get
			} // TooManyDefaults

			public bool RejectStep {
				get { return TooManyDefaults && (MaxConsumerScore < MaxConsumerScoreThreshhold); } // get
			} // RejectStep
		} // class DataModel
	} // class ConsumerDefaults
} // namespace
