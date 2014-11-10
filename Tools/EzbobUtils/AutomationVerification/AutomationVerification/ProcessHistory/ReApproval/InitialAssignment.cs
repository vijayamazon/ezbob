﻿namespace AutomationCalculator.ProcessHistory.ReApproval {
	using System.Collections.Generic;

	public class InitialAssignment : ATrace {
		public InitialAssignment(int nCustomerID, bool bCompletedSuccessfully) : base(nCustomerID, bCompletedSuccessfully) {
			ValidationErrors = new List<string>();
		} // constructor

		public void Init(List<string> oValidationErrors) {
			ValidationErrors.AddRange(oValidationErrors);

			if (CompletedSuccessfully)
				Comment = string.Format("customer {0} data has been fully loaded", CustomerID);
			else {
				Comment = string.Format(
					"customer {0} data has not been fully loaded: {1}",
					CustomerID,
					string.Join("; ", ValidationErrors)
				);
			} // if
		} // Init

		public List<string> ValidationErrors { get; private set; }
	}  // class InitialAssignment
} // namespace
