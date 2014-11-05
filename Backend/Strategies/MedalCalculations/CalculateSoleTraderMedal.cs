namespace EzBob.Backend.Strategies.MedalCalculations 
{
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class CalculateSoleTraderMedal : AStrategy
	{
		private readonly SoleTraderMedalDualCalculator soleTraderMedalDualCalculator;
		private readonly int customerId;

		public CalculateSoleTraderMedal(AConnection db, ASafeLog log, int customerId)
			: base(db, log)
		{
			this.customerId = customerId;
			soleTraderMedalDualCalculator = new SoleTraderMedalDualCalculator(db, log);
		}

		public override string Name {
			get { return "CalculateSoleTraderMedal"; }
		}

		public ScoreResult Result { get; private set; }

		public override void Execute()
		{
			Result = soleTraderMedalDualCalculator.CalculateMedalScore(customerId, DateTime.UtcNow);
		}
	}
}
