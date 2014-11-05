namespace EzBob.Backend.Strategies.MedalCalculations 
{
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class CalculateNonLimitedMedal : AStrategy
	{
		private readonly NonLimitedMedalDualCalculator nonLimitedMedalDualCalculator;
		private readonly int customerId;

		public CalculateNonLimitedMedal(AConnection db, ASafeLog log, int customerId)
			: base(db, log)
		{
			this.customerId = customerId;
			nonLimitedMedalDualCalculator = new NonLimitedMedalDualCalculator(db, log);
		}

		public override string Name {
			get { return "CalculateNonLimitedMedal"; }
		}

		public ScoreResult Result { get; private set; }

		public override void Execute()
		{
			Result = nonLimitedMedalDualCalculator.CalculateMedalScore(customerId, DateTime.UtcNow);
		}
	}
}
