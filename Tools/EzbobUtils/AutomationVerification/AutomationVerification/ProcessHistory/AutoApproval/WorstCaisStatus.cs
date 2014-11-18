namespace AutomationCalculator.ProcessHistory.AutoApproval {
	using System.Collections.Generic;
	using System.Linq;

	public class WorstCaisStatus : ATrace {
		public WorstCaisStatus(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		public List<string> FoundForbiddenStatuses { get; private set; }
		public List<string> AllCustomerStatuses { get; private set; }
		public List<string> AllowedStatuses { get; private set; }

		public void Init(IEnumerable<string> oFoundForbiddenStatuses, IEnumerable<string> oAllCustomerStatuses, IEnumerable<string> oAllowedStatuses) {
			FoundForbiddenStatuses = oFoundForbiddenStatuses == null ? new List<string>() : oFoundForbiddenStatuses.ToList();
			AllCustomerStatuses = oAllCustomerStatuses == null ? new List<string>() : oAllCustomerStatuses.ToList();
			AllowedStatuses = oAllowedStatuses == null ? new List<string>() : oAllowedStatuses.ToList();

			if (FoundForbiddenStatuses.Count < 1) {
				Comment = string.Format(
					"no forbidden statuses found among '{0}'; allowed statuses are '{1}'",
					string.Join(", ", AllCustomerStatuses),
					string.Join(", ", AllowedStatuses)
				);
			}
			else {
				Comment = string.Format(
					"forbidden statuses found '{2}' among '{0}'; allowed statuses are '{1}'",
					string.Join(", ", AllCustomerStatuses),
					string.Join(", ", AllowedStatuses),
					string.Join(", ", FoundForbiddenStatuses)
				);
			} // if
		} // Init
	} // class WorstCaisStatus
} // namespace