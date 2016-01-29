namespace Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions {
	public abstract class AAutoDecisionBase : IDecisionCheckAgent {
		public virtual bool WasMismatch {
			get { return this.wasMismatch; }
			protected set { this.wasMismatch = value; }
		} // WasMismatch

		public abstract void MakeAndVerifyDecision();

		public abstract bool WasException { get; }

		public abstract bool AffirmativeDecisionMade { get; }

		protected AAutoDecisionBase() {
			// To be on the safe side.
			// In case of mismatch automation is aborted and manual decision should be made.
			this.wasMismatch = true;
		} // constructor

		private bool wasMismatch;
	} // class AAutoDecisionBase
} // namespace
