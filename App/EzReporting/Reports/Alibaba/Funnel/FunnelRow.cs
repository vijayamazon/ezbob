namespace Reports.Alibaba.Funnel {
	using System.Collections.Generic;
	using Ezbob.Utils;

	class FunnelRow : RejectReasonRow {
		[NonTraversable]
		public virtual int? DropOff { get; set; }

		public virtual bool DoDropOff { get; set; }

		public override IEnumerable<object> GetAdditionalReportFields() {
			return DropOff.HasValue
				? new List<object> { DropOff, Pct }
				: new List<object> { string.Empty, string.Empty };
		} // GetAdditionalReportFields
	} // class FunnelRow
} // namespace
