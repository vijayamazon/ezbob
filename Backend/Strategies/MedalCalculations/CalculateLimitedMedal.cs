namespace EzBob.Backend.Strategies.MedalCalculations 
{
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

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

		public ScoreResult Result { get; private set; }

		public override void Execute()
		{
			Result = limitedMedalDualCalculator.CalculateMedalScore(customerId, DateTime.UtcNow);
		}
	}
}
