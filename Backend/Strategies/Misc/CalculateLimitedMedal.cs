namespace EzBob.Backend.Strategies.Misc 
{
	using Ezbob.Database;
	using Ezbob.Logger;
	using LimitedMedalCalculation;

	public class CalculateLimitedMedal : AStrategy
	{
		private readonly LimitedMedalDualCalculator limitedMedalDualCalculator;
		private readonly int customerId;

		public CalculateLimitedMedal(AConnection db, ASafeLog log, int customerId)
			: base(db, log)
		{
			this.customerId = customerId;
			limitedMedalDualCalculator = new LimitedMedalDualCalculator(db, log);
		}

		public override string Name {
			get { return "CalculateLimitedMedal"; }
		}

		public override void Execute()
		{
			limitedMedalDualCalculator.CalculateMedalScore(customerId);
		}
	}
}
