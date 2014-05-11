namespace EzBob.Backend.Strategies.PricingModel
{
	public class PricingModelModel
	{
		// Main input fields
		public decimal LoanAmount { get; set; }
		public decimal DefaultRate { get; set; }
		public decimal DefaultRateCompanyShare { get; set; }
		public decimal DefaultRateCustomerShare { get; set; }
		public decimal SetupFeePounds { get; set; }
		public decimal SetupFeePercents { get; set; }

		// Other input fields
		public int LoanTerm { get; set; }
		public int InterestOnlyPeriod { get; set; }
		public decimal TenurePercents { get; set; }
		public decimal TenureMonths { get; set; }
		public decimal CollectionRate { get; set; }
		public decimal Cogs { get; set; }
		public decimal DebtPercentOfCapital { get; set; }
		public decimal CostOfDebt { get; set; }
		public decimal OpexAndCapex { get; set; }
		public decimal ProfitMarkup { get; set; }

		// Main output fields
		public decimal MonthlyInterestRate { get; set; }
		public decimal SetupFeeForEuLoan { get; set; }
		public decimal EuLoanPercentages { get; set; } // 1.75 or 2

		// Other output fields
		public decimal Revenue { get; set; }
		public decimal CogsOutput { get; set; }
		public decimal GrossProfit { get; set; }
		public decimal OpexAndCapexOutput { get; set; }
		public decimal Ebitda { get; set; }
		public decimal NetLossFromDefaults { get; set; }
		public decimal CostOfDebtOutput { get; set; }
		public decimal TotalCost { get; set; }
		public decimal ProfitMarkupOutput { get; set; }
		public decimal AnnualizedInterestRate { get; set; }
		public decimal Apr { get; set; }
	}
}