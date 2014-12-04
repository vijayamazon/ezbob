﻿namespace EzBob.Backend.Strategies.OfferCalculation 
{
	using System;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Database;
	using Ezbob.Logger;
	using MedalCalculations;

	public class CalculateOffer : AStrategy
	{
		private readonly OfferDualCalculator offerDualCalculator;
		private readonly int customerId;
		private readonly int amount;
		private readonly bool hasLoans;
		private readonly Medal medalClassification;

		public CalculateOffer(int customerId, int amount, bool hasLoans, Medal medalClassification, AConnection db, ASafeLog log)
			: base(db, log)
		{
			offerDualCalculator = new OfferDualCalculator(db, log);
			this.customerId = customerId;
			this.amount = amount;
			this.hasLoans = hasLoans;
			this.medalClassification = medalClassification;
		}

		public override string Name {
			get { return "CalculateOffer"; }
		}

		public OfferResult Result { get; private set; }

		public override void Execute()
		{
			Result = offerDualCalculator.CalculateOffer(customerId, DateTime.UtcNow, amount, hasLoans, medalClassification);
		}
	}
}
