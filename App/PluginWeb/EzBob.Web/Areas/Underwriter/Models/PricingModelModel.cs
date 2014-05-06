namespace EzBob.Web.Areas.Underwriter.Models
{
	public class PricingModelModel
	{
		// Main input fields
		public int LoanAmount { get; set; }
		public decimal InterestRate { get; set; }
		public decimal DefaultRate { get; set; }
		public decimal InitiationFee { get; set; }

		// Other input fields
		public int LoanTerm { get; set; }
		public int InterestOnlyPeriod { get; set; }
		public decimal TenureAsPercentOfLoanTerm { get; set; }
		public int TenureAsMonthsOfLoanTerm { get; set; }
		public decimal CollectionRate { get; set; }
		public int Cogs { get; set; }
		public decimal DebtPercentOfCapital { get; set; }
		public decimal CostOfDebt { get; set; }
		public int OpexAndCapex { get; set; }
		public int ProfitBeforeTax { get; set; }

		// Main output fields
		public decimal MonthlyInterestToCharge { get; set; }
		public int SetupFeeForEuLoan { get; set; }
		public decimal EuLoanPercentages { get; set; } // 1.75 or 2

		// Other output fields
		public int AverageLoanAmount { get; set; }
		public int AverageRevenuePerLoan { get; set; }
		public int CogsOutput { get; set; }
		public int GrossProfit { get; set; }
		public int OpexAndCapexOutput { get; set; }
		public int Ebitda { get; set; }
		public int NetLossFromDefaults { get; set; }
		public int CostOfDebtOutput { get; set; }
		public int TotalCost { get; set; }
		public int ProfitBeforeTaxOutput { get; set; }
		public int Balance { get; set; }
		
		public PricingModelModel(int customerId)
		{
			LoanAmount = customerId;
		}
	}
}