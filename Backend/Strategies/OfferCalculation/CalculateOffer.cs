namespace EzBob.Backend.Strategies.OfferCalculation 
{
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;
	using MedalCalculations;

	public class CalculateOffer : AStrategy
	{
		private readonly OfferDualCalculator offerDualCalculator;
		private readonly int customerId;
		private readonly int amount;
		private readonly MedalClassification medalClassification;

		public CalculateOffer(int customerId, int amount, MedalClassification medalClassification, AConnection db, ASafeLog log)
			: base(db, log)
		{
			offerDualCalculator = new OfferDualCalculator(db, log);
			this.customerId = customerId;
			this.amount = amount;
			this.medalClassification = medalClassification;
		}

		public override string Name {
			get { return "CalculateOffer"; }
		}

		public OfferResult Result { get; private set; }

		public override void Execute()
		{
			Result = offerDualCalculator.CalculateOffer(customerId, DateTime.UtcNow, amount, medalClassification);
		}
	}
}
