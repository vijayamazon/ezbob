namespace EzBob.Backend.Strategies.PricingModel
{
	using Ezbob.Database;
	using Ezbob.Logger;
	using Models;

	public class PricingModelCalculate : AStrategy
	{
		private readonly int customerId;

		public PricingModelCalculate(int customerId, AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
			this.customerId = customerId;
		}

		public override string Name {
			get { return "Pricing model calculate"; }
		}

		public PricingModelModel Model { get; private set; }
		
		public override void Execute() {
			// TODO: Calculation logic
			Model = new PricingModelModel();
			Model.MonthlyInterestToCharge = 1.23m;
			Model.OpexAndCapexOutput = customerId;
		}
	}
}
