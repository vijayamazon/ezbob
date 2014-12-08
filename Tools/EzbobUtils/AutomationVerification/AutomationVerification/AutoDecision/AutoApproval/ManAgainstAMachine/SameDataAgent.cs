namespace AutomationCalculator.AutoDecision.AutoApproval.ManAgainstAMachine {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class SameDataAgent : Agent {
		public SameDataAgent(
			int nCustomerID,
			decimal nSystemCalculatedAmount,
			AutomationCalculator.Common.Medal nMedal,
			DateTime now,
			AConnection oDB,
			ASafeLog oLog
		) : base(nCustomerID, nSystemCalculatedAmount, nMedal, oDB, oLog) {
			this.now = now;
		} // constructor

		public override Agent Init() {
			base.Init();

			Now = this.now;

			return this;
		} // Init

		protected override Configuration InitCfg() {
			return new SameDataConfiguration(DB, Log);
		} // InitCfg

		protected override void GatherAvailableFunds() {
			// Just dummy values (for now) so that approval ain't no breaks on this.
			Funds.Available = 1000000m;
			Funds.Reserved = 1000m;
		} // GatherAvailableFunds

		private readonly DateTime now;
	} // class Agent
} // namespace
