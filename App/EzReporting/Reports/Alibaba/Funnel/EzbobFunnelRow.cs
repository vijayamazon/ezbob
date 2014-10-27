namespace Reports.Alibaba.Funnel {
	using System.Collections.Generic;
	using Ezbob.Utils;

	class EzbobFunnelRow : RejectReasonRow {
		[NonTraversable]
		public virtual int? DropOff { get; set; }

		public override IEnumerable<object> GetAdditionalReportFields() {
			if (DropOff.HasValue)
				return new List<object> { DropOff, Pct };
			else
				return new List<object> { string.Empty, string.Empty };
		} // GetAdditionalReportFields
	} // class EzbobFunnelRow
} // namespace
