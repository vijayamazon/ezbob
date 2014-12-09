namespace EzBob.Backend.Strategies.MainStrategy.AutoDecisions.Reject.Turnover {
	using System;

	internal abstract class ASimplePeriodValue : IPeriodValue {

		public void Add(Row r) {
			if (IsMy(r))
				Value += r.Turnover;
		} // Add

		public decimal Value { get; private set; }

		protected ASimplePeriodValue() {
			Value = 0;
		} // constructor

		protected abstract bool IsMy(Row r);

		protected static readonly Guid Yodlee = new Guid("107DE9EB-3E57-4C5B-A0B5-FFF445C4F2DF");
		protected static readonly Guid Hmrc   = new Guid("AE85D6FC-DBDB-4E01-839A-D5BD055CBAEA");

	} // class ASimplePeriodValue
} // namespace
