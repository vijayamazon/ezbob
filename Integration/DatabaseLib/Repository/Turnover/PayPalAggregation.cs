namespace EZBob.DatabaseLib.Repository.Turnover {
	using System;

	public class PayPalAggregation {
		public long PayPalAggregationID { get; set; }
		public DateTime TheMonth { get; set; }
		public bool IsActive { get; set; }
		public decimal Turnover { get; set; }
		public decimal GrossIncome { get; set; }
		public int NetNumOfRefundsAndReturns { get; set; }
		public decimal NetSumOfRefundsAndReturns { get; set; }
		public decimal NetTransfersAmount { get; set; }
		public int NumOfTotalTransactions { get; set; }
		public int NumTransfersIn { get; set; }
		public int NumTransfersOut { get; set; }
		public decimal OutstandingBalance { get; set; }
		public decimal RatioNetSumOfRefundsAndReturnsToNetRevenuesDenominator { get; set; }
		public decimal RatioNetSumOfRefundsAndReturnsToNetRevenuesNumerator { get; set; }
		public decimal RevenuesForTransactions { get; set; }
		public decimal TotalNetExpenses { get; set; }
		public decimal TotalNetInPayments { get; set; }
		public decimal TotalNetOutPayments { get; set; }
		public decimal TotalNetRevenues { get; set; }
		public int TransactionsNumber { get; set; }
		public decimal TransferAndWireIn { get; set; }
		public decimal TransferAndWireOut { get; set; }
		public decimal AmountPerTransferInNumerator { get; set; }
		public decimal AmountPerTransferInDenominator { get; set; }
		public decimal AmountPerTransferOutNumerator { get; set; }
		public decimal AmountPerTransferOutDenominator { get; set; }
		public decimal GrossProfitMarginNumerator { get; set; }
		public decimal GrossProfitMarginDenominator { get; set; }
		public decimal RevenuePerTrasnactionNumerator { get; set; }
		public decimal RevenuePerTrasnactionDenominator { get; set; }
	} // class PayPalAggregation
} // namespace
