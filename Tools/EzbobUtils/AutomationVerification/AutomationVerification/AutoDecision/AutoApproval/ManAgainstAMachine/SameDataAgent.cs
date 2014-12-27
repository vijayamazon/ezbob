namespace AutomationCalculator.AutoDecision.AutoApproval.ManAgainstAMachine {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class SameDataAgent : Agent {
		public SameDataAgent(
			int nCustomerID,
			decimal nSystemCalculatedAmount,
			AutomationCalculator.Common.Medal nMedal,
			AutomationCalculator.Common.MedalType nMedalType,
			DateTime now,
			AConnection oDB,
			ASafeLog oLog
			)
			: base(nCustomerID, nSystemCalculatedAmount, nMedal, nMedalType, oDB, oLog) {
			this.now = now;
		} // constructor

		public override Agent Init() {
			base.Init();

			Now = now;

			return this;
		} // Init

		protected override void GatherAvailableFunds() {
			// Just dummy values (for now) so that approval ain't no breaks on this.
			Funds.Available = 1000000m;
			Funds.Reserved = 1000m;
		}

		protected override Configuration InitCfg() {
			return new SameDataConfiguration(DB, Log);
		} // InitCfg

		// GatherAvailableFunds

		private readonly DateTime now;
	} // class Agent
} // namespace
