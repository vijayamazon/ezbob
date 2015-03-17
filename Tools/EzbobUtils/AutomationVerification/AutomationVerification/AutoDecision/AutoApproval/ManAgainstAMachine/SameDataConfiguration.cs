namespace AutomationCalculator.AutoDecision.AutoApproval.ManAgainstAMachine {
	using Ezbob.Database;
	using Ezbob.Logger;

	public class SameDataConfiguration : Configuration {
		public SameDataConfiguration() {} // constructor

		public SameDataConfiguration(AConnection db, ASafeLog log) : base(db, log) {
		} // constructor

		public override void Load() {
			base.Load();

			// This class is used to check auto approval for old cash requests so
			// it always runs in silent mode to prevent approved sum limitations.
			IsSilent = true;
		} // Load
	} // class Configuration
} // namespace
