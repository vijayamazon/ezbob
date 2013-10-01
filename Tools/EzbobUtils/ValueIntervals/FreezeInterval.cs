using System;

namespace Ezbob.ValueIntervals {
	#region class FreezeInterval

	public class FreezeInterval : DateInterval {
		#region public

		#region operator * (intersection)

		public static FreezeInterval operator *(FreezeInterval a, FreezeInterval b) {
			if (ReferenceEquals(a, b))
				return b;

			if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
				return null;

			return a.Intersection(b);
		} // operator *

		#endregion operator * (intersection)

		#region constructor

		public FreezeInterval(DateTime? oStart, DateTime? oEnd, decimal? nInterestRate) : base(oStart, oEnd) {
			InterestRate = nInterestRate;
		} // constructor

		#endregion constructor

		#region property InterestRate

		public decimal? InterestRate { get; private set; }

		#endregion property InterestRate

		#region method Intersection

		public virtual FreezeInterval Intersection(FreezeInterval other) {
			if (other == null)
				return null;

			DateInterval oEdges = base.Intersection(other);

			if (oEdges == null)
				return null;

			return new FreezeInterval(oEdges, other.InterestRate);
		} // Intersection

		#endregion method Intersection

		#endregion public

		#region protected

		#region constructor

		protected FreezeInterval(DateInterval oDateInterval, decimal? nInterestRate) : base(oDateInterval) {
			InterestRate = nInterestRate;
		} // constructor

		#endregion constructor

		#endregion protected
	} // class FreezeInterval

	#endregion class FreezeInterval
} // namespace Ezbob.ValueIntervals
