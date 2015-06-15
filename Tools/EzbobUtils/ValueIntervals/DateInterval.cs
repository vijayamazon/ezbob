namespace Ezbob.ValueIntervals {
	using System;
	using System.Collections.Generic;

	using EdgeType = Ezbob.ValueIntervals.AIntervalEdge<System.DateTime>.EdgeType;

	public class DateInterval : TInterval<DateTime> {

		public static DateTime Min(DateTime a, DateTime b) { return (a.Date <= b.Date) ? a.Date : b.Date; } // Min
		public static DateTime Max(DateTime a, DateTime b) { return (a.Date >= b.Date) ? a.Date : b.Date; } // Max
		
		public DateInterval(DateTime? oLeft, DateTime? oRight) : base(InitEdges(oLeft, oRight)) {
		} // constructor
		
		protected DateInterval(TInterval<DateTime> other) : base(other.Left, other.Right) {
		} // constructor

		protected DateInterval(AIntervalEdge<DateTime> oLeft, AIntervalEdge<DateTime> oRight) : base(oLeft, oRight) {
		} // constructor

		public virtual bool IsJustBefore(DateInterval other) {
			return Right.IsFinite && other.Left.IsFinite && (Right.Value.AddDays(1) == other.Left.Value);
		} // IsJustBefore

		public virtual bool Contains(DateTime oDate) {
			return Contains(new DateIntervalEdge(oDate, EdgeType.Finite));
		} // Contains

		public static DateInterval operator *(DateInterval a, DateInterval b) {
			return ReferenceEquals(a, null) ? null : a.Intersection(b);
		} // operator *

		public static TDisjointIntervals<DateTime> operator -(DateInterval a, DateInterval b) {
			return ReferenceEquals(a, null) ? null : a.Difference(b);
		} // operator -

		protected virtual DateInterval Intersection(DateInterval other) {
			TInterval<DateTime> oEdges = base.Intersection(other);

			return oEdges == null ? null : new DateInterval(oEdges);
		} // Intersection

		protected virtual TDisjointIntervals<DateTime> Difference(DateInterval other) {
			TDisjointIntervals<DateTime> oDiff = base.Difference(other);

			if (oDiff == null)
				return null;

			var oResult = new TDisjointIntervals<DateTime>();

			foreach (TInterval<DateTime> i in oDiff)
				oResult.Add(new DateInterval(i));

			return oResult;
		} // Difference

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

		public override string ToString() {
			return string.Format("[{0}, {1}]", Left, Right);
		} // ToString

	} // class DateInterval

	public static class DateIntervalListExt {
		public static string SortWithoutCheckSequence(this List<DateInterval> oDates) {
			if (oDates.Count > 1)
				oDates.Sort((a, b) => a.Left.CompareTo(b.Left));

			return null;
		} // SortWithoutCheckSequence
	} // class DateIntervalListExt

} // namespace
