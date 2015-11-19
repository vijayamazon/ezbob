namespace EZBob.DatabaseLib.Repository.Turnover {
	using System;

	public class FreeAgentAggregation {
		public long FreeAgentAggregationID { get; set; }
		public DateTime TheMonth { get; set; }
		public bool IsActive { get; set; }
		public decimal Turnover { get; set; }
		public int NumOfExpenses { get; set; }
		public int NumOfOrders { get; set; }
		public decimal SumOfAdminExpensesCategory { get; set; }
		public decimal SumOfCostOfSalesExpensesCategory { get; set; }
		public decimal SumOfDraftInvoices { get; set; }
		public decimal SumOfGeneralExpensesCategory { get; set; }
		public decimal SumOfOpenInvoices { get; set; }
		public decimal SumOfOverdueInvoices { get; set; }
		public decimal SumOfPaidInvoices { get; set; }
		public decimal TotalSumOfExpenses { get; set; }
		public decimal TotalSumOfOrders { get; set; }
	} // FreeAgentAggregation
} // namespace