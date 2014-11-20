namespace AutomationCalculator.ProcessHistory.ReApproval {
	using System.Collections.Generic;

	public class InitialAssignment : ATrace {
		public InitialAssignment(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
			ValidationErrors = new List<string>();
		} // constructor

		public void Init(List<string> oValidationErrors) {
			if (oValidationErrors != null)
				ValidationErrors.AddRange(oValidationErrors);

			if (DecisionStatus == DecisionStatus.Affirmative)
				Comment = string.Format("customer data has been fully loaded");
			else {
				Comment = string.Format(
					"customer data has not been fully loaded: {0}",
					string.Join("; ", ValidationErrors)
				);
			} // if
		} // Init

		public List<string> ValidationErrors { get; private set; }
	}  // class InitialAssignment
} // namespace
