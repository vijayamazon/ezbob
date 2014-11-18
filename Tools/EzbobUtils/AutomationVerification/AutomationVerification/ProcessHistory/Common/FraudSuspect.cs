namespace AutomationCalculator.ProcessHistory.Common {
	using EZBob.DatabaseLib.Model.Database;

	public class FraudSuspect : ATrace {
		#region constructor

		public FraudSuspect(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		#endregion constructor

		public void Init(FraudStatus nFraudStatus) {
			FraudStatus = nFraudStatus;

			Comment = string.Format("fraud status is {0}", FraudStatus);
		} // Init

		public FraudStatus FraudStatus { get; private set; }
	} // class FraudSuspect
} // namespace
