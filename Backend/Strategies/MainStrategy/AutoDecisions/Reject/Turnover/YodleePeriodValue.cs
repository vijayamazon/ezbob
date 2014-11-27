namespace EzBob.Backend.Strategies.MainStrategy.AutoDecisions.Reject.Turnover {
	internal class YodleePeriodValue : ASimplePeriodValue {
		protected override bool IsMy(Row r) {
			return r.MpTypeInternalID == Yodlee;
		} // IsMy
	} // class YodleePeriodValue
} // namespace
