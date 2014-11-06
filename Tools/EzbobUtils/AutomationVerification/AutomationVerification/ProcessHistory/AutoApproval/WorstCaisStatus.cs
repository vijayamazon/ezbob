namespace AutomationCalculator.ProcessHistory.AutoApproval {
	using System.Collections.Generic;

	public class WorstCaisStatus : ATrace {
		public WorstCaisStatus(int nCustomerID, bool bCompletedSuccessfully) : base(nCustomerID, bCompletedSuccessfully) {
		} // constructor

		public List<string> FoundForbiddenStatuses { get; private set; }
		public List<string> AllCustomerStatuses { get; private set; }
		public List<string> AllowedStatuses { get; private set; }

		public void Init(List<string> oFoundForbiddenStatuses, List<string> oAllCustomerStatuses, List<string> oAllowedStatuses) {
			FoundForbiddenStatuses = oFoundForbiddenStatuses ?? new List<string>();
			AllCustomerStatuses = oAllCustomerStatuses ?? new List<string>();
			AllowedStatuses = oAllowedStatuses ?? new List<string>();

			if (FoundForbiddenStatuses.Count < 1) {
				Comment = string.Format(
					"customer {0} no forbidden statuses found among '{1}'; allowed statuses are '{2}'",
					CustomerID,
					string.Join(", ", AllCustomerStatuses),
					string.Join(", ", AllowedStatuses)
				);
			}
			else {
				Comment = string.Format(
					"customer {0} forbidden statuses found '{3}' among '{1}'; allowed statuses are '{2}'",
					CustomerID,
					string.Join(", ", AllCustomerStatuses),
					string.Join(", ", AllowedStatuses),
					string.Join(", ", FoundForbiddenStatuses)
				);
			} // if
		} // Init
	} // class WorstCaisStatus
} // namespace