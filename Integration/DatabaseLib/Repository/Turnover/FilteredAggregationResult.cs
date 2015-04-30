namespace EZBob.DatabaseLib.Repository.Turnover {
	using System;

	public class FilteredAggregationResult {

		public virtual DateTime TheMonth { get; set; }
		public virtual decimal Turnover { get; set; }
		public virtual int MpId { get; set; }
		public virtual int Distance { get; set; }

	}
}
