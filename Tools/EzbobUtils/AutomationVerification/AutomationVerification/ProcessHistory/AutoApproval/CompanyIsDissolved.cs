namespace AutomationCalculator.ProcessHistory.AutoApproval {
	using System;
	using System.Globalization;

	public class CompanyIsDissolved : ATrace {
		public CompanyIsDissolved(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		public void Init(DateTime? dissolutionDate) {
			if (dissolutionDate == null)
				Comment = "Company is not dissolved.";
			else {
				Comment = string.Format(
					"Company has been dissolved on '{0}'",
					dissolutionDate.Value.ToString("MMMM d yyyy", CultureInfo.InvariantCulture)
				);
			} // if
		} // Init
	} // class CompanyIsDissolved
} // namespace