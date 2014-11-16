namespace AutomationCalculator.ProcessHistory.AutoApproval {
	using System.Collections.Generic;
	using System.Text;
	using Newtonsoft.Json;

	public class InitialAssignment : ATrace {
		public InitialAssignment(int nCustomerID, DecisionStatus nDecisionStatus) : base(nCustomerID, nDecisionStatus) {
			ValidationErrors = new List<string>();
		} // constructor

		public void Init(decimal nOfferedCreditLine, IEnumerable<string> oValidationErrors = null) {
			if (oValidationErrors != null)
				ValidationErrors.AddRange(oValidationErrors);

			OfferedCreditLine = nOfferedCreditLine;

			var os = new StringBuilder();

			os.AppendFormat("customer {0} system calculated sum is {1}", CustomerID, OfferedCreditLine);

			if (DecisionStatus == DecisionStatus.Affirmative)
				os.Append("; data has been fully loaded");
			else
				os.AppendFormat("; data has not been fully loaded: {0}", string.Join("; ", ValidationErrors));

			Comment = os.ToString();
		} // Init

		public decimal OfferedCreditLine { get; private set; }

		public List<string> ValidationErrors { get; private set; }

		public override string GetInitArgs() {
			return JsonConvert.SerializeObject(new List<object> { OfferedCreditLine, ValidationErrors });
		} // GetInitArgs
	}  // class InitialAssignment
} // namespace
