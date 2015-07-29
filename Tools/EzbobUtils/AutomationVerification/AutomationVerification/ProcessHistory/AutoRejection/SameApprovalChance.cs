namespace AutomationCalculator.ProcessHistory.AutoRejection {
	public class SameApprovalChance : ATrace {
		public SameApprovalChance(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		public void Init(bool primaryHasChance, bool verificationHasChance) {
			Comment = string.Format(
				"Both implementations {0}agree on customer's approval chance: " +
				"has {1} chance in primary, has {2} chance in verification.",
				(primaryHasChance == verificationHasChance) ? string.Empty : "dis",
				primaryHasChance ? "a" : "no",
				verificationHasChance ? "a" : "no"
			);
		} // Init
	} // class SameApprovalChance
} // namespace
