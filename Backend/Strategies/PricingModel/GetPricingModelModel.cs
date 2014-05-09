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
					DefaultRate = 0.04m,
					SetupFeePounds = 150,
					SetupFeePercents = 0.07m,
					LoanTerm = 12,
					InterestOnlyPeriod = 3,
					TenurePercents = 0.5m,
					TenureMonths = 0.223m,
					CollectionRate = 6.88m,
					Cogs = 1000,
					DebtPercentOfCapital = 0.65m,
					CostOfDebt = 0.65m,
					OpexAndCapex = 180,
					ProfitBeforeTax = 291
				};
		}
	}
}
