namespace Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions {
	public abstract class AAutoDecisionBase {
		public bool WasMismatch { get; protected set; }

		protected AAutoDecisionBase() {
			// To be on the safe side.
			// In case of mismatch automation is aborted and manual decision should be made.
			WasMismatch = true;
		} // constructor
	} // class AAutoDecisionBase
} // namespace
