namespace AutomationCalculator.ProcessHistory.AutoRejection
{
	public class Turnover: ATrace
	{
		public Turnover(DecisionStatus nDecisionStatus) : base(nDecisionStatus) { }

		public DataModel Model { get; private set; }

		public void Init(DataModel model) {
			Model = model;
			Comment = string.Format(
				"customer annual turnover {0} (min allowed {1}) quarter turnover {2} (min allowed {3}) and has {4} company files",
				model.AnnualTurnover,
				model.AnnualTurnoverThreshhold,
				model.QuarterTurnover,
				model.QuarterTurnoverThreshhold,
				model.HasCompanyFiles ? "a" : "no");
		} // Init

		public class DataModel {
			public bool HasCompanyFiles { get; set; }
			public int AnnualTurnover { get; set; }
			public int AnnualTurnoverThreshhold { get; set; }
			public int QuarterTurnover { get; set; }
			public int QuarterTurnoverThreshhold { get; set; }
		}
	}
}
