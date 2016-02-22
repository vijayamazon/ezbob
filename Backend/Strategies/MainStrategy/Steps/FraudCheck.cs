namespace Ezbob.Backend.Strategies.MainStrategy.Steps {
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Strategies.Misc;

	internal class FraudCheck : AOneExitStep {
		public FraudCheck(
			string outerContextDescription,
			int customerID,
			bool customerIsTest
		) : base(outerContextDescription) {
			this.customerID = customerID;
			this.customerIsTest = customerIsTest;
		} // constructor

		protected override void ExecuteStep() {
			if (!this.customerIsTest)
				new FraudChecker(this.customerID, FraudMode.FullCheck).Execute();
		} // ExecuteStep

		private readonly int customerID;
		private readonly bool customerIsTest;
	} // class FraudCheck
} // namespace
