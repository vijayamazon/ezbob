namespace EZBob.DatabaseLib.Repository.Turnover {
	using System;

	public class HmrcAggregation {
		public long HmrcAggregationID { get; set; }
		public DateTime TheMonth { get; set; }
		public bool IsActive { get; set; }
		public decimal Turnover { get; set; }
		public decimal ValueAdded { get; set; }
		public decimal FreeCashFlow { get; set; }
	} // class HmrcAggregation
} // namespace
