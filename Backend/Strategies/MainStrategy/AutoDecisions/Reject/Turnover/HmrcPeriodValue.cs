namespace EzBob.Backend.Strategies.MainStrategy.AutoDecisions.Reject.Turnover {
	internal class HmrcPeriodValue : ASimplePeriodValue {
		protected override bool IsMy(Row r) {
			return r.MpTypeInternalID == Hmrc;
		} // IsMy
	} // class HmrcPeriodValue
} // namespace
