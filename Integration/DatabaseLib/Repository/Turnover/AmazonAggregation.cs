namespace EZBob.DatabaseLib.Repository.Turnover {
	using System;

	public class AmazonAggregation {
		public long AmazonAggregationID { get; set; }
		public DateTime TheMonth { get; set; }
		public bool IsActive { get; set; }
		public decimal Turnover { get; set; }
		public int AverageItemsPerOrderDenominator { get; set; }
		public int AverageItemsPerOrderNumerator { get; set; }
		public decimal AverageSumOfOrderDenominator { get; set; }
		public decimal AverageSumOfOrderNumerator { get; set; }
		public int CancelledOrdersCount { get; set; }
		public int NumOfOrders { get; set; }
		public decimal OrdersCancellationRateDenominator { get; set; }
		public decimal OrdersCancellationRateNumerator { get; set; }
		public int TotalItemsOrdered { get; set; }
		public decimal TotalSumOfOrders { get; set; }
	} // class AmazonAggregation
} // namespace