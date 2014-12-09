namespace Ezbob.Backend.Strategies.MainStrategy.AutoDecisions.Reject.Turnover {
	using System;

	internal class AccountingPeriodValue : ASimplePeriodValue {
		protected override bool IsMy(Row r) {
			return
				r.IsPaymentAccount &&
				(r.MpTypeInternalID != PayPal) &&
				(r.MpTypeInternalID != Yodlee) &&
				(r.MpTypeInternalID != Hmrc);
		} // IsMy

		private static readonly Guid PayPal = new Guid("3FA5E327-FCFD-483B-BA5A-DC1815747A28");
	} // class AccountingPeriodValue
} // namespace
