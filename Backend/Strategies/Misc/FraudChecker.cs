namespace EzBob.Backend.Strategies.Misc {
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using global::FraudChecker;

	public class FraudChecker : AStrategy {
		public FraudChecker(int customerId, FraudMode mode, AConnection oDb, ASafeLog oLog) : base(oDb, oLog) {
			this.customerId = customerId;
			this.mode = mode;
		} // constructor

		public override string Name {
			get { return "Fraud Checker"; }
		} // Name

		public override void Execute() {
			var checker = new FraudDetectionChecker();
			checker.Check(customerId, mode);
		} // Execute

		private readonly int customerId;
		private readonly FraudMode mode;
	} // class FraudChecker
} // namespace
