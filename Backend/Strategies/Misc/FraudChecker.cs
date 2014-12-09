namespace Ezbob.Backend.Strategies.Misc {
	using Ezbob.Backend.Models;
	using global::FraudChecker;

	public class FraudChecker : AStrategy {
		public FraudChecker(int customerId, FraudMode mode) {
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
