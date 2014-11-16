namespace AutomationCalculator.ProcessHistory {
	public enum DecisionStatus {
		/// <summary>
		/// No decision has been made so far.
		/// </summary>
		Dunno = 0,

		/// <summary>
		/// Decision has been made, and according to path it is:
		/// Re-reject: reject
		/// Reject: reject
		/// Re-approve: approve
		/// Approve: approve
		/// </summary>
		Affirmative = 1,

		/// <summary>
		/// Decision has been made, and according to path it is:
		/// Re-reject: do not reject
		/// Reject: do not reject
		/// Re-approve: do not approve
		/// Approve: do not approve
		/// </summary>
		Negative = 2,
	} // enum DecisionStatus
} // namespace
