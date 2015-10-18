namespace Ezbob.Backend.Models {
	using System;

	public class FilteredAggregationResult {
		public virtual DateTime TheMonth { get; set; }
		public virtual decimal Turnover { get; set; }
		public virtual int MpId { get; set; }
		public virtual int Distance { get; set; }
	} // class FilteredAggregationResult
} // namespace
