using System;

namespace Ezbob.ValueIntervals {
	#region class ValueIntervalException

	public class ValueIntervalException : Exception {
		#region public

		#region constructor

		public ValueIntervalException(string sMsg) : base(sMsg) {} // constructor

		public ValueIntervalException(string sMsg, Exception oInner) : base(sMsg, oInner) {} // constructor

		#endregion constructor

		#endregion public
	} // class ValueIntervalException

	#endregion class ValueIntervalException
} // namespace Ezbob.ValueIntervals
