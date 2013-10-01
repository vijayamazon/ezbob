using System;

using EdgeType = Ezbob.ValueIntervals.AIntervalEdge<System.DateTime>.EdgeType;

namespace Ezbob.ValueIntervals {
	#region class DateInterval

	public class DateInterval : TInterval<DateTime> {
		#region public

		#region constructor

		public DateInterval(DateTime? oLeft, DateTime? oRight) : base(InitEdges(oLeft, oRight)) {
		} // constructor

		#endregion constructor

		#region method Intersection

		public virtual DateInterval Intersection(DateInterval other) {
			if (other == null)
				return null;

			TInterval<DateTime> oEdges = base.Intersection(other);

			if (oEdges == null)
				return null;

			return new DateInterval(oEdges);
		} // Intersection

		#endregion method Intersection

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

		#endregion constructor

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

		private static DateTime Min(DateTime a, DateTime b) { return (a.Date <= b.Date) ? a.Date : b.Date; } // Min
		private static DateTime Max(DateTime a, DateTime b) { return (a.Date >= b.Date) ? a.Date : b.Date; } // Max

		#endregion private
	} // class DateInterval

	#endregion class DateInterval
} // namespace Ezbob.ValueIntervals
