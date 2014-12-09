using System;

namespace Ezbob.ValueIntervals {

	public class FreezeInterval : DateInterval {

		public static FreezeInterval operator *(FreezeInterval a, FreezeInterval b) {
			return ReferenceEquals(a, null) ? null : a.Intersection(b);
		} // operator *

		public static TDisjointIntervals<DateTime> operator -(FreezeInterval a, FreezeInterval b) {
			return ReferenceEquals(a, null) ? null : a.Difference(b);
		} // operator -

		public FreezeInterval(DateTime? oStart, DateTime? oEnd, decimal? nInterestRate) : base(oStart, oEnd) {
			InterestRate = nInterestRate;
		} // constructor

		public decimal? InterestRate { get; private set; }

		public override string ToString() { return string.Format("[ {0} - {1} ({2:P2})]", Left, Right, InterestRate); } // ToString

		protected FreezeInterval(DateInterval oDateInterval, decimal? nInterestRate) : base(oDateInterval) {
			InterestRate = nInterestRate;
		} // constructor

		protected virtual FreezeInterval Intersection(FreezeInterval other) {
			DateInterval oEdges = base.Intersection(other);

			return oEdges == null ? null : new FreezeInterval(oEdges, InterestRate);
		} // Intersection

		protected virtual TDisjointIntervals<DateTime> Difference(FreezeInterval other) {
			TDisjointIntervals<DateTime> oDiff = base.Difference(other);

			if (oDiff == null)
				return null;

			var oResult = new TDisjointIntervals<DateTime>();

			foreach (TInterval<DateTime> i in oDiff)
				oResult.Add(new FreezeInterval(i as DateInterval, InterestRate));

			return oResult;
		} // Difference

	} // class FreezeInterval

} // namespace Ezbob.ValueIntervals
