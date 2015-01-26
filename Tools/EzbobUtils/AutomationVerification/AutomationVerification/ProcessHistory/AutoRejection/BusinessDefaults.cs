namespace AutomationCalculator.ProcessHistory.AutoRejection {
	public class BusinessDefaults : ATrace {
		public BusinessDefaults(DecisionStatus nDecisionStatus) : base(nDecisionStatus) { }

		public DataModel Model { get; private set; }

		public void Init(DataModel model) {
			Model = model;

			Comment = string.Format(
				"customer has {0} default accounts ({1} allowed) with default amount of {2} (allowed {3})" +
				"and business score of {4} (allowed {5})",
				model.NumOfDefaults, model.NumDefaultAccountsThreshhold,
				model.AmountOfDefaults, model.AmountDefaultAccountsThreshhold,
				model.MaxBusinessScore, model.MaxBusinessScoreThreshhold
			);
		} // Init

		public class DataModel {
			public int MaxBusinessScore { get; set; }
			public int NumOfDefaults { get; set; }
			public int AmountOfDefaults { get; set; }

			public int MaxBusinessScoreThreshhold { get; set; }
			public int NumDefaultAccountsThreshhold { get; set; }
			public int AmountDefaultAccountsThreshhold { get; set; }

			public bool RejectStep {
				get {
					return
						(NumOfDefaults>= NumDefaultAccountsThreshhold) &&
						(MaxBusinessScore < MaxBusinessScoreThreshhold);
				} // get
			} // RejectStep
		} // class DataModel
	} // class BusinessDefaults
} // namespace
