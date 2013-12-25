namespace EzBob.Backend.Strategies {
	using Ezbob.Database;
	using Ezbob.Logger;
	using global::FraudChecker;

	public class FraudChecker : AStrategy {
		public FraudChecker(int customerId, AConnection oDb, ASafeLog oLog) : base(oDb, oLog) {
			this.customerId = customerId;
		} // constructor

		public override string Name {
			get { return "Fraud Checker"; }
		} // Name

		public override void Execute() {
			var checker = new FraudDetectionChecker();
			checker.Check(customerId);
		} // Execute

		private readonly int customerId;
	} // class FraudChecker
} // namespace
