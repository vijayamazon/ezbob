using System;

namespace Ezbob.Logger {
	#region class LogException

	public class LogException : Exception {
		#region public

		#region constructor

		public LogException(string sMsg) : base(sMsg) {} // constructor

		public LogException(string sMsg, Exception oInner) : base(sMsg, oInner) {} // constructor

		#endregion constructor

		#endregion public
	} // class LogException

	#endregion class LogException
} // namespace Logger
