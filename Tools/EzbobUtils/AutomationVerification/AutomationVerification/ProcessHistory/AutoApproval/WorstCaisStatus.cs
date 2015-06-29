namespace AutomationCalculator.ProcessHistory.AutoApproval {
	using System.Collections.Generic;
	using System.Linq;

	public class WorstCaisStatus : ATrace {
		public WorstCaisStatus(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		public void Init(IEnumerable<string> oFoundForbiddenStatuses) {
			List<string> lst = oFoundForbiddenStatuses == null ? new List<string>() : oFoundForbiddenStatuses.ToList();

			Comment = lst.Count < 1
				? "no forbidden statuses found"
				: string.Format("forbidden statuses found '{0}'", string.Join(", ", lst));
		} // Init
	} // class WorstCaisStatus
} // namespace