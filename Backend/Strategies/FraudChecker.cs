namespace EzBob.Backend.Strategies
{
	using log4net;
	using global::FraudChecker;

	public class FraudChecker
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(FraudChecker));

		private readonly int customerId;

		public FraudChecker(int customerId)
		{
			this.customerId = customerId;
		}

		public void Execute()
		{
			var checker = new FraudDetectionChecker();
			checker.Check(customerId);
		}
	}
}
