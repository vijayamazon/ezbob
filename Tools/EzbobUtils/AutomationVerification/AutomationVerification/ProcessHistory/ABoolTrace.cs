namespace AutomationCalculator.ProcessHistory {
	using System.Collections.Generic;
	using Newtonsoft.Json;

	public abstract class ABoolTrace : ATrace {
		public virtual ATrace Init() {
			HasProperty = DecisionStatus == DecisionStatus.Affirmative;

			Comment = string.Format("customer {0} has {1}{2}", CustomerID, HasProperty ? string.Empty : "no ", PropertyName);

			return this;
		} // Init

		public virtual bool HasProperty { get; private set; }

		public override string GetInitArgs() {
			return JsonConvert.SerializeObject(new List<string>());
		} // GetInitArgs

		protected ABoolTrace(int nCustomerID, DecisionStatus nDecisionStatus) : base(nCustomerID, nDecisionStatus) {
		} // constructor

		protected abstract string PropertyName { get; }
	}  // class ABoolTrace
} // namespace
