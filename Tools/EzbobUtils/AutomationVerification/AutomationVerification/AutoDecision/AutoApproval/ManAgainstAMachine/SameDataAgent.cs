namespace AutomationCalculator.AutoDecision.AutoApproval.ManAgainstAMachine {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class SameDataAgent : Agent {
		public SameDataAgent(
			int nCustomerID,
			long? cashRequestID,
			long? nlCashRequestID,
			decimal nSystemCalculatedAmount,
			AutomationCalculator.Common.Medal nMedal,
			AutomationCalculator.Common.MedalType nMedalType,
			AutomationCalculator.Common.TurnoverType? turnoverType,
			DateTime now,
			AConnection oDB,
			ASafeLog oLog
		) : base(
			nCustomerID,
			cashRequestID,
			nlCashRequestID,
			nSystemCalculatedAmount,
			nMedal,
			nMedalType,
			turnoverType,
			oDB,
			oLog
		) {
			this.now = now;
		} // constructor

		public override Agent Init() {
			base.Init();

			Now = this.now;

			return this;
		} // Init

		protected override void GatherAvailableFunds() {
			// Just dummy values (for now) so that approval ain't no breaks on this.
			Funds.Available = 1000000m;
			Funds.Reserved = 1000m;
		} // GatherAvailableFunds

		protected override Configuration InitCfg() {
			return new SameDataConfiguration(DB, Log);
		} // InitCfg

		private readonly DateTime now;
	} // class Agent
} // namespace
