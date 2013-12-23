using Ezbob.Database;
using Ezbob.Logger;
using FraudChecker;

namespace EzBob.Backend.Strategies {
	public class FraudChecker : AStrategy {
		public FraudChecker(int customerId, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
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
