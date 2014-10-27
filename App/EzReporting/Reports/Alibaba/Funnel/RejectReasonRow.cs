namespace Reports.Alibaba.Funnel {
	using System.Collections.Generic;
	using Ezbob.Utils;

	internal class RejectReasonRow : StrInt {
		[NonTraversable]
		public virtual double Pct { get; set; }

		public override IEnumerable<object> GetAdditionalReportFields() {
			return new List<object> { Pct };
		} // GetAdditionalReportFields
	} // class RejectReasonRow
} // namespace
