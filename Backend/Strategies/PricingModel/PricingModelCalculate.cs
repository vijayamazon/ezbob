namespace Ezbob.Backend.Strategies.PricingModel {
	using Exceptions;

	public class PricingModelCalculate : AStrategy {
		public PricingModelCalculate(int customerId, PricingModelModel model) {
			Model = model;
			this.pricingModelCalculator = new PricingModelCalculator(customerId, model);
		} // constructor

		public override string Name { get { return "Pricing model calculate"; } }

		public PricingModelModel Model { get; private set; }

		public override void Execute() {
			this.pricingModelCalculator.Execute();

			if (!string.IsNullOrEmpty(this.pricingModelCalculator.Error))
				throw new StrategyWarning(this, this.pricingModelCalculator.Error);

			Model = this.pricingModelCalculator.Model;
		} // Execute

		private readonly PricingModelCalculator pricingModelCalculator;
	} // class PricingModelCalculate
} // namespace
