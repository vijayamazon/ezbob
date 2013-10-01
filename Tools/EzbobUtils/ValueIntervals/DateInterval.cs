using System;

using EdgeType = Ezbob.ValueIntervals.AIntervalEdge<System.DateTime>.EdgeType;

namespace Ezbob.ValueIntervals {
	#region class DateInterval

	public class DateInterval {
		#region public

		#region method CompareForSort

		public static int CompareForSort(DateInterval a, DateInterval b) { return a.Left.CompareTo(b.Left); } // CompareForSort

		#endregion method CompareForSort

		#region constructor

		public DateInterval(DateTime? oLeft, DateTime? oRight) {
			if (oLeft.HasValue && oRight.HasValue) {
				Left = new DateIntervalEdge(Min(oLeft.Value, oRight.Value), EdgeType.Finite);
				Right = new DateIntervalEdge(Max(oLeft.Value, oRight.Value), EdgeType.Finite);
				return;
			} // if

			Left = new DateIntervalEdge(oLeft, EdgeType.NegativeInfinity);
			Right = new DateIntervalEdge(oRight, EdgeType.PositiveInfinity);
		} // constructor

		#endregion constructor

		#region method Intersects

		public bool Intersects(DateInterval di) {
			if ((Left <= di.Left) && (di.Right <= Right))
				return true;

			if ((di.Left <= Left) && (Right <= di.Right))
				return true;

			if ((Left <= di.Left) && (di.Left <= Right))
				return true;

			if ((Left <= di.Right) && (di.Right <= Right))
				return true;

			return false;
		} // Intersects

		#endregion method Intersects

		#region method Follows

		public bool Follows(DateInterval di) {
			if ((Right.Type != EdgeType.Finite) || (di.Left.Type != EdgeType.Finite))
				return false;

			return Right.Value.AddDays(1) == di.Left.Value;
		} // Follows

		#endregion method Follows

		#region method ToString

		public override string ToString() { return string.Format("[ {0} - {1} ]", Left, Right); } // ToString

		#endregion method ToString

		#region property Left

		public DateIntervalEdge Left { get; private set; }

		#endregion property Left

		#region property Right

		public DateIntervalEdge Right { get; private set; }

		#endregion property Right

		#endregion public

		#region private

		private static DateTime Min(DateTime a, DateTime b) { return (a.Date <= b.Date) ? a.Date : b.Date; } // Min
		private static DateTime Max(DateTime a, DateTime b) { return (a.Date >= b.Date) ? a.Date : b.Date; } // Max

		#endregion private
	} // class DateInterval

	#endregion class DateInterval
} // namespace Ezbob.ValueIntervals
