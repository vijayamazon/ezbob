using System;

namespace Ezbob.ValueIntervals {
	#region class FreezeInterval

	public class FreezeInterval : DateInterval {
		#region public

		#region constructor

		public FreezeInterval(DateTime? oStart, DateTime? oEnd, decimal? nInterestRate) : base(oStart, oEnd) {
			InterestRate = nInterestRate;
		} // constructor

		#endregion constructor

		#region property InterestRate

		public decimal? InterestRate { get; private set; }

		#endregion property InterestRate

		#endregion public
	} // class FreezeInterval

	#endregion class FreezeInterval
} // namespace Ezbob.ValueIntervals
