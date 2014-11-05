namespace EzBob.Backend.Strategies.MedalCalculations
{
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class CalculateOnlineLimitedMedal : AStrategy
	{
		private readonly OnlineLimitedMedalDualCalculator onlineLimitedMedalDualCalculator;
		private readonly int customerId;

		public CalculateOnlineLimitedMedal(AConnection db, ASafeLog log, int customerId)
			: base(db, log)
		{
			this.customerId = customerId;
			onlineLimitedMedalDualCalculator = new OnlineLimitedMedalDualCalculator(db, log);
		}

		public override string Name {
			get { return "CalculateOnlineLimitedMedal"; }
		}

		public ScoreResult Result { get; private set; }

		public override void Execute()
		{
			Result = onlineLimitedMedalDualCalculator.CalculateMedalScore(customerId, DateTime.UtcNow);
		}
	}
}
