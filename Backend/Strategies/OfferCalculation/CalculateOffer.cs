namespace Ezbob.Backend.Strategies.OfferCalculation {
	using System;
	using EZBob.DatabaseLib.Model.Database;

	public class CalculateOffer : AStrategy {
		public CalculateOffer(int customerId, int amount, bool hasLoans, Medal medalClassification) {
			this.offerDualCalculator = new OfferDualCalculator(
				customerId,
				DateTime.UtcNow,
				amount,
				hasLoans,
				medalClassification
			);
		}

		public override string Name {
			get { return "CalculateOffer"; }
		}

		public OfferResult Result { get; private set; }

		public override void Execute() {
			Result = this.offerDualCalculator.CalculateOffer();
		}

		private readonly OfferDualCalculator offerDualCalculator;
	}
}
