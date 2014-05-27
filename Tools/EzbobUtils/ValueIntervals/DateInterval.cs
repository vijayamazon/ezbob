using System;

using EdgeType = Ezbob.ValueIntervals.AIntervalEdge<System.DateTime>.EdgeType;

namespace Ezbob.ValueIntervals {
	using System.Collections.Generic;

	#region class DateInterval

	public class DateInterval : TInterval<DateTime> {
		#region public

		public static DateTime Min(DateTime a, DateTime b) { return (a.Date <= b.Date) ? a.Date : b.Date; } // Min
		public static DateTime Max(DateTime a, DateTime b) { return (a.Date >= b.Date) ? a.Date : b.Date; } // Max

		#region operator *

		public static DateInterval operator *(DateInterval a, DateInterval b) {
			return ReferenceEquals(a, null) ? null : a.Intersection(b);
		} // operator *

		#endregion operator *

		#region operator -

		public static TDisjointIntervals<DateTime> operator -(DateInterval a, DateInterval b) {
			return ReferenceEquals(a, null) ? null : a.Difference(b);
		} // operator -

		#endregion operator -

		#region constructor

		public DateInterval(DateTime? oLeft, DateTime? oRight) : base(InitEdges(oLeft, oRight)) {
		} // constructor

		#endregion constructor

		#region method IsJustBefore

		public virtual bool IsJustBefore(DateInterval other) {
			return Right.IsFinite && other.Left.IsFinite && (Right.Value.AddDays(1) == other.Left.Value);
		} // IsJustBefore

		#endregion method IsJustBefore

		#endregion public

		#region protected

		#region constructor

		protected DateInterval(TInterval<DateTime> other) : base(other.Left, other.Right) {
		} // constructor

		protected DateInterval(AIntervalEdge<DateTime> oLeft, AIntervalEdge<DateTime> oRight) : base(oLeft, oRight) {
		} // constructor

		#endregion constructor

		#region method Intersection

		protected virtual DateInterval Intersection(DateInterval other) {
			TInterval<DateTime> oEdges = base.Intersection(other);

			return oEdges == null ? null : new DateInterval(oEdges);
		} // Intersection

		#endregion method Intersection

		#region method Difference

		protected virtual TDisjointIntervals<DateTime> Difference(DateInterval other) {
			TDisjointIntervals<DateTime> oDiff = base.Difference(other);

			if (oDiff == null)
				return null;

			var oResult = new TDisjointIntervals<DateTime>();

			foreach (TInterval<DateTime> i in oDiff)
				oResult.Add(new DateInterval(i));

			return oResult;
		} // Difference

		#endregion method Difference

		#endregion protected

		#region private

		#region method InitEdges

		private static Tuple<AIntervalEdge<DateTime>, AIntervalEdge<DateTime>> InitEdges(DateTime? oLeft, DateTime? oRight) {
			if (oLeft.HasValue && oRight.HasValue) {
				var l = new DateIntervalEdge(Min(oLeft.Value, oRight.Value), EdgeType.Finite);
				var r = new DateIntervalEdge(Max(oLeft.Value, oRight.Value), EdgeType.Finite);

				return new Tuple<AIntervalEdge<DateTime>, AIntervalEdge<DateTime>>(l, r);
			} // if

			return new Tuple<AIntervalEdge<DateTime>, AIntervalEdge<DateTime>>(
				new DateIntervalEdge(oLeft, EdgeType.NegativeInfinity),
				new DateIntervalEdge(oRight, EdgeType.PositiveInfinity)
			);
		} // InitEdges

		#endregion method InitEdges

		#endregion private
	} // class DateInterval

	#endregion class DateInterval

	#region class DateIntervalListExt

	public static class DateIntervalListExt {
		public static string SortAndCheckSequence(this List<DateInterval> oDates) {
			string sResult = null;

			if (oDates.Count > 1) {
				oDates.Sort((a, b) => a.Left.CompareTo(b.Left));

				DateInterval next = null;

				foreach (DateInterval cur in oDates) {
					if (next == null) {
						next = cur;
						continue;
					} // if

					DateInterval prev = next;
					next = cur;

					if (!prev.IsJustBefore(next)) {
						sResult = "In-consequent date ranges: " + prev + " and " + next + ".";
						break;
					} // if
				} // for each interval
			} // if

			return sResult;
		} // SortAndCheckSequence

		public static string SortWithoutCheckSequence(this List<DateInterval> oDates) {
			if (oDates.Count > 1)
				oDates.Sort((a, b) => a.Left.CompareTo(b.Left));

			return null;
		} // SortWithoutCheckSequence
	} // class DateIntervalListExt

	#endregion class DateIntervalListExt
} // namespace Ezbob.ValueIntervals
