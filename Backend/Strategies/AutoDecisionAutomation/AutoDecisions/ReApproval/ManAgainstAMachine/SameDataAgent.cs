namespace Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.ReApproval.ManAgainstAMachine {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class SameDataAgent : Agent {
		public SameDataAgent(
			int customerID,
			long? cashRequestID,
			long? nlCashRequestID,
			DateTime now,
			string tag,
			AConnection db,
			ASafeLog log
		) : base(customerID, cashRequestID, nlCashRequestID, tag, db, log) {
			this.now = now; // Not assigning directly to Now to avoid "virtual call in constructor".
		} // constructor

		public override Agent Init() {
			base.Init();

			Now = this.now;

			return this;
		} // Init

		public virtual void Decide() {
			RunPrimary();

			Trail.Save(DB, null);
		} // Decide

		protected override void GatherAvailableFunds() {
			// Just dummy values (for now) so that approval ain't no breaks on this.
			Funds.Available = 1000000m;
			Funds.Reserved = 1000m;
		} // GatherAvailableFunds

		private readonly DateTime now;
	} // class SameDataAgent
} // namespace
