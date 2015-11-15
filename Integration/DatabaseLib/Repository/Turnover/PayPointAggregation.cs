namespace EZBob.DatabaseLib.Repository.Turnover {
	using System;

	public class PayPointAggregation {
		public long PayPointAggregationID { get; set; }
		public DateTime TheMonth { get; set; }
		public bool IsActive { get; set; }
		public decimal Turnover { get; set; }
		public decimal CancellationRateDenominator { get; set; }
		public decimal CancellationRateNumerator { get; set; }
		public decimal CancellationValue { get; set; }
		public int NumOfFailures { get; set; }
		public int NumOfOrders { get; set; }
		public decimal OrdersAverageDenominator { get; set; }
		public decimal OrdersAverageNumerator { get; set; }
		public decimal SumOfAuthorisedOrders { get; set; }
	} // PayPointAggregation
} // namespace