namespace EzBob.Backend.Strategies.PricingModel
{
	using Ezbob.Database;
	using Ezbob.Logger;

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
			Model = new PricingModelModel
				{
					LoanAmount = customerId,
					DefaultRate = 1.3m,
					InitiationFee = 150,
					LoanTerm = 12,
					InterestOnlyPeriod = 3,
					TenurePercents = 55.1m,
					TenureMonths = 2.2m,
					CollectionRate = 6.88m,
					Cogs = 1000,
					DebtPercentOfCapital = 6.51m,
					CostOfDebt = 0.65m,
					OpexAndCapex = 180,
					ProfitBeforeTax = 291
				};
		}
	}
}
