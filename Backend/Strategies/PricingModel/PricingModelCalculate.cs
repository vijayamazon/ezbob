namespace EzBob.Backend.Strategies.PricingModel
{
	using Exceptions;
	using Ezbob.Database;
	using Ezbob.Logger;
	
	public class PricingModelCalculate : AStrategy
	{
		private readonly PricingModelCalculator pricingModelCalculator;

		public PricingModelCalculate(int customerId, PricingModelModel model, AConnection db, ASafeLog log)
			: base(db, log)
		{
			Model = model;
			pricingModelCalculator = new PricingModelCalculator(customerId, model, db,log);
		}

		public override string Name {
			get { return "Pricing model calculate"; }
		}

		public PricingModelModel Model { get; private set; }

		public override void Execute(){
			pricingModelCalculator.Execute();
			if (!string.IsNullOrEmpty(pricingModelCalculator.Error))
			{
				throw new StrategyWarning(this, pricingModelCalculator.Error);
			}
			Model = pricingModelCalculator.Model;

			Log.Info("Pricing Model Result: \n {0}", Model);
		}
	}
}
