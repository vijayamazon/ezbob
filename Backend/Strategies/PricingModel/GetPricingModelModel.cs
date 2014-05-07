namespace EzBob.Backend.Strategies.PricingModel
{
	using Ezbob.Database;
	using Ezbob.Logger;
	using Models;

	public class GetPricingModelModel : AStrategy
	{
		private readonly int customerId;

		public GetPricingModelModel(int customerId, AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
			this.customerId = customerId;
		}

		public override string Name {
			get { return "Get pricing model model"; }
		}

		public PricingModelModel Model { get; private set; }
		
		public override void Execute() {
			// TODO: Fill model with real data
			Model = new PricingModelModel();
			Model.LoanAmount = customerId;
			Model.InterestRate = 3.21m;
		}
	}
}
