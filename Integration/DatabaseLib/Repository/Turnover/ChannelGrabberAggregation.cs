namespace EZBob.DatabaseLib.Repository.Turnover {
	using System;

	public class ChannelGrabberAggregation {
		public long ChannelGrabberAggregationID { get; set; }
		public DateTime TheMonth { get; set; }
		public bool IsActive { get; set; }
		public decimal Turnover { get; set; }
		public decimal AverageSumOfExpensesDenominator { get; set; }
		public decimal AverageSumOfExpensesNumerator { get; set; }
		public decimal AverageSumOfOrdersDenominator { get; set; }
		public decimal AverageSumOfOrdersNumerator { get; set; }
		public int NumOfExpenses { get; set; }
		public int NumOfOrders { get; set; }
		public decimal TotalSumOfExpenses { get; set; }
		public decimal TotalSumOfOrders { get; set; }
	} // ChannelGrabberAggregation
} // namespace
