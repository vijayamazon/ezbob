using System;

namespace Ezbob.ValueIntervals {

	public class ValueIntervalException : Exception {

		public ValueIntervalException(string sMsg) : base(sMsg) {} // constructor

		public ValueIntervalException(string sMsg, Exception oInner) : base(sMsg, oInner) {} // constructor

	} // class ValueIntervalException

} // namespace Ezbob.ValueIntervals
