using System;

using EdgeType = Ezbob.ValueIntervals.AIntervalEdge<System.DateTime>.EdgeType;

namespace Ezbob.ValueIntervals {
	#region class FreezeInterval

	class FreezeInterval {
		#region public

		#region constructor

		public FreezeInterval(DateTime? oStart, DateTime? oEnd, decimal? nInterestRate) {
			if (oStart.HasValue && oEnd.HasValue && (oStart.Value.CompareTo(oEnd.Value) < 0)) {
				DateTime tmp = oStart.Value;
				oStart = oEnd;
				oEnd = tmp;
			} // if

			Left = new FreezeIntervalEdge(oStart, EdgeType.NegativeInfinity);
			Right = new FreezeIntervalEdge(oEnd, EdgeType.PositiveInfinity);
			InterestRate = nInterestRate;
		} // constructor

		#endregion constructor

		#region property Left

		public FreezeIntervalEdge Left { get; private set; }

		#endregion property Left

		#region property Right

		public FreezeIntervalEdge Right { get; private set; }

		#endregion property Right

		#region property InterestRate

		public decimal? InterestRate { get; private set; }

		#endregion property InterestRate

		#region method Intersect
		#endregion method Intersect

		#endregion public
	} // class FreezeInterval

	#endregion class FreezeInterval
} // namespace Ezbob.ValueIntervals
