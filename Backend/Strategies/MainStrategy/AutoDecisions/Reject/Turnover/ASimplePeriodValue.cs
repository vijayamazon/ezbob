namespace EzBob.Backend.Strategies.MainStrategy.AutoDecisions.Reject.Turnover {
	using System;

	internal abstract class ASimplePeriodValue : IPeriodValue {
		#region public

		#region method Add

		public void Add(Row r) {
			if (IsMy(r))
				Value += r.Turnover;
		} // Add

		#endregion method Add

		public decimal Value { get; private set; }

		#endregion public

		#region protected

		#region constructor

		protected ASimplePeriodValue() {
			Value = 0;
		} // constructor

		#endregion constructor

		protected abstract bool IsMy(Row r);

		protected static readonly Guid Yodlee = new Guid("107DE9EB-3E57-4C5B-A0B5-FFF445C4F2DF");
		protected static readonly Guid Hmrc   = new Guid("AE85D6FC-DBDB-4E01-839A-D5BD055CBAEA");

		#endregion protected
	} // class ASimplePeriodValue
} // namespace
