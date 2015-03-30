namespace Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.ReApproval.ManAgainstAMachine {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class SameDataAgent : Agent {
		public SameDataAgent(int customerID, DateTime now, AConnection db, ASafeLog log) : base(customerID, db, log) {
			this.now = now; // Not assigning directly to Now to avoid "virtual call in constructor".
		} // constructor

		public override Agent Init() {
			base.Init();

			Now = this.now;

			return this;
		} // Init

		public virtual void Decide(bool saveTrail, long? cashRequestID = null, string tag = null) {
			RunPrimary();

			if (saveTrail)
				Trail.Save(DB, null, cashRequestID, tag);
		} // Decide

		protected override void GatherAvailableFunds() {
			// Just dummy values (for now) so that approval ain't no breaks on this.
			Funds.Available = 1000000m;
			Funds.Reserved = 1000m;
		} // GatherAvailableFunds

		private readonly DateTime now;
	} // class SameDataAgent
} // namespace
