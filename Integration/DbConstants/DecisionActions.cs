namespace DbConstants {
	public enum DecisionActions {
		Approve = 1,
		Reject = 2,
		Escalate = 3,
		Pending = 4,
		Waiting = 5,
		ReApprove = 6,
		ReReject = 7,
	} // enum DecisionActions

	public static class DecisionActionsExt {
		public static bool In(this DecisionActions? da, params DecisionActions[] set) {
			return (da != null) && da.Value.In(set);
		} // In

		public static bool In(this DecisionActions da, params DecisionActions[] set) {
			foreach (var item in set)
				if (da == item)
					return true;

			return false;
		} // In
	} // class DecisionActionsExt
} // namespace
