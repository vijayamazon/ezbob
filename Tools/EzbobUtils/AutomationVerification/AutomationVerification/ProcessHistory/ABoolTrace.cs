namespace AutomationCalculator.ProcessHistory {
	public abstract class ABoolTrace : ATrace {
		public virtual ATrace Init() {
			return Init(DecisionStatus == DecisionStatus.Affirmative);
		} // Init

		public virtual ATrace Init(bool value) {
			HasProperty = value;

			Comment = string.Format("customer has {0}{1}", HasProperty ? string.Empty : "no ", PropertyName);

			return this;
		} // Init

		public virtual bool HasProperty { get; private set; }

		protected ABoolTrace(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		protected abstract string PropertyName { get; }
	}  // class ABoolTrace
} // namespace
