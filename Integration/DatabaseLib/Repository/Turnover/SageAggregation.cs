namespace EZBob.DatabaseLib.Repository.Turnover {
	using System;

	public class SageAggregation {
		public long SageAggregationID { get; set; }
		public DateTime TheMonth { get; set; }
		public bool IsActive { get; set; }
		public decimal Turnover { get; set; }
		public int NumOfExpenditures { get; set; }
		public int NumOfIncomes { get; set; }
		public int NumOfOrders { get; set; }
		public int NumOfPurchaseInvoices { get; set; }
		public decimal TotalSumOfExpenditures { get; set; }
		public decimal TotalSumOfIncomes { get; set; }
		public decimal TotalSumOfOrders { get; set; }
		public decimal TotalSumOfPaidPurchaseInvoices { get; set; }
		public decimal TotalSumOfPaidSalesInvoices { get; set; }
		public decimal TotalSumOfPartiallyPaidPurchaseInvoices { get; set; }
		public decimal TotalSumOfPartiallyPaidSalesInvoices { get; set; }
		public decimal TotalSumOfPurchaseInvoices { get; set; }
		public decimal TotalSumOfUnpaidPurchaseInvoices { get; set; }
		public decimal TotalSumOfUnpaidSalesInvoices { get; set; }
	} // SageAggregation
} // namespace