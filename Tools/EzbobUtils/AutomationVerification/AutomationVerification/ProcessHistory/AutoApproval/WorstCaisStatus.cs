namespace AutomationCalculator.ProcessHistory.AutoApproval {
	using System.Collections.Generic;
	using System.Linq;
	using Newtonsoft.Json;

	public class WorstCaisStatus : ATrace {
		public WorstCaisStatus(int nCustomerID, DecisionStatus nDecisionStatus) : base(nCustomerID, nDecisionStatus) {
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

		public override string GetInitArgs() {
			return JsonConvert.SerializeObject(new List<IEnumerable<string>> { FoundForbiddenStatuses, AllCustomerStatuses, AllowedStatuses });
		} // GetInitArgs
	} // class WorstCaisStatus
} // namespace