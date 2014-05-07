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
			Model = new PricingModelModel
				{
					MonthlyInterestToCharge = 1.23m,
					SetupFeeForEuLoan = 400,
					EuLoanPercentages = 1.75m,
					AverageLoanAmount = 4500,
					AverageRevenuePerLoan = 87,
					CogsOutput = 50,
					GrossProfit = 432,
					OpexAndCapexOutput = 66844,
					Ebitda = 11332,
					NetLossFromDefaults = 8912,
					CostOfDebtOutput = 345,
					TotalCost = 34700,
					ProfitBeforeTaxOutput = 5000,
					Balance = 4567
				};
		}
	}
}
