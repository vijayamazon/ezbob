namespace EZBob.DatabaseLib.Repository.Turnover {
	using System;

	public class EkmAggregation {
		public long EkmAggregationID { get; set; }
		public DateTime TheMonth { get; set; }
		public bool IsActive { get; set; }
		public decimal Turnover { get; set; }
		public decimal AverageSumOfCancelledOrderDenominator { get; set; }
		public decimal AverageSumOfCancelledOrderNumerator { get; set; }
		public decimal AverageSumOfOrderDenominator { get; set; }
		public decimal AverageSumOfOrderNumerator { get; set; }
		public decimal AverageSumOfOtherOrderDenominator { get; set; }
		public decimal AverageSumOfOtherOrderNumerator { get; set; }
		public decimal CancellationRateDenominator { get; set; }
		public decimal CancellationRateNumerator { get; set; }
		public int NumOfCancelledOrders { get; set; }
		public int NumOfOrders { get; set; }
		public int NumOfOtherOrders { get; set; }
		public decimal TotalSumOfCancelledOrders { get; set; }
		public decimal TotalSumOfOrders { get; set; }
		public decimal TotalSumOfOtherOrders { get; set; }
	} // EkmAggregation
} // namespace