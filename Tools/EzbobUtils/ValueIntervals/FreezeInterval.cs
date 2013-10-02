using System;

namespace Ezbob.ValueIntervals {
	#region class FreezeInterval

	public class FreezeInterval : DateInterval {
		#region public

		#region operator * (intersection)

		public static FreezeInterval operator *(FreezeInterval a, FreezeInterval b) {
			return ReferenceEquals(a, null) ? null : a.Intersection(b);
		} // operator *

		#endregion operator * (intersection)

		#region operator -

		public static TDisjointIntervals<DateTime> operator -(FreezeInterval a, FreezeInterval b) {
			return ReferenceEquals(a, null) ? null : a.Difference(b);
		} // operator -

		#endregion operator -

		#region constructor

		public FreezeInterval(DateTime? oStart, DateTime? oEnd, decimal? nInterestRate) : base(oStart, oEnd) {
			InterestRate = nInterestRate;
		} // constructor

		#endregion constructor

		#region property InterestRate

		public decimal? InterestRate { get; private set; }

		#endregion property InterestRate

		#region method ToString

		public override string ToString() { return string.Format("[ {0} - {1} ({2:P2})]", Left, Right, InterestRate); } // ToString

		#endregion method ToString

		#endregion public

		#region protected

		#region constructor

		protected FreezeInterval(DateInterval oDateInterval, decimal? nInterestRate) : base(oDateInterval) {
			InterestRate = nInterestRate;
		} // constructor

		#endregion constructor

		#region method Intersection

		protected virtual FreezeInterval Intersection(FreezeInterval other) {
			DateInterval oEdges = base.Intersection(other);

			return oEdges == null ? null : new FreezeInterval(oEdges, InterestRate);
		} // Intersection

		#endregion method Intersection

		#region method Difference

		protected virtual TDisjointIntervals<DateTime> Difference(FreezeInterval other) {
			TDisjointIntervals<DateTime> oDiff = base.Difference(other);

			if (oDiff == null)
				return null;

			var oResult = new TDisjointIntervals<DateTime>();

			foreach (TInterval<DateTime> i in oDiff)
				oResult.Add(new FreezeInterval(i as DateInterval, InterestRate));

			return oResult;
		} // Difference

		#endregion method Difference

		#endregion protected
	} // class FreezeInterval

	#endregion class FreezeInterval
} // namespace Ezbob.ValueIntervals
