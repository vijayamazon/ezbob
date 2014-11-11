namespace AutomationCalculator.ProcessHistory.AutoApproval {
	using System.Collections.Generic;
	using System.Text;

	public class InitialAssignment : ATrace {
		public InitialAssignment(int nCustomerID, bool bCompletedSuccessfully) : base(nCustomerID, bCompletedSuccessfully) {
			ValidationErrors = new List<string>();
		} // constructor

		public void Init(decimal nOfferedCreditLine, List<string> oValidationErrors = null) {
			if (oValidationErrors != null)
				ValidationErrors.AddRange(oValidationErrors);

			OfferedCreditLine = nOfferedCreditLine;

			var os = new StringBuilder();

			os.AppendFormat("customer {0} system calculated sum is {1}", CustomerID, OfferedCreditLine);

			if (CompletedSuccessfully)
				os.Append("; data has been fully loaded");
			else
				os.AppendFormat("; data has not been fully loaded: {0}", string.Join("; ", ValidationErrors));

			Comment = os.ToString();
		} // Init

		public decimal OfferedCreditLine { get; private set; }

		public List<string> ValidationErrors { get; private set; }
	}  // class InitialAssignment
} // namespace
