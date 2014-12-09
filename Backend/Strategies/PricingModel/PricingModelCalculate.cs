namespace Ezbob.Backend.Strategies.PricingModel {
	using Exceptions;

	public class PricingModelCalculate : AStrategy {
		private readonly PricingModelCalculator pricingModelCalculator;

		public PricingModelCalculate(int customerId, PricingModelModel model) {
			Model = model;
			pricingModelCalculator = new PricingModelCalculator(customerId, model, DB, Log);
		}

		public override string Name {
			get { return "Pricing model calculate"; }
		}

		public PricingModelModel Model { get; private set; }

		public override void Execute() {
			pricingModelCalculator.Calculate();

			if (!string.IsNullOrEmpty(pricingModelCalculator.Error))
				throw new StrategyWarning(this, pricingModelCalculator.Error);

			Model = pricingModelCalculator.Model;
		}
	}
}
