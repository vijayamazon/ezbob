namespace AutomationCalculator.ProcessHistory.AutoApproval {
	using Newtonsoft.Json;

	public class Turnover : AThresholdTrace {
		public Turnover(int nCustomerID, DecisionStatus nDecisionStatus) : base(nCustomerID, nDecisionStatus) {
		} // constructor

		public virtual string PeriodName { get; set; }

		public override string GetProperties() {
			return JsonConvert.SerializeObject(new { PeriodName = PeriodName, });
		} // GetProperties

		protected override string ValueName {
			get { return PeriodName + " turnover"; }
		} // ValueName
	}  // class Turnover
} // namespace
