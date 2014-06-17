namespace Ezbob.Utils.Exceptions {
	using System;
	using Ezbob.Logger;

	public class Warning : ALoggedException {
		public Warning(ASafeLog oLog, string sMsg, Exception oInnerException = null) : base(Severity.Warn, oLog, sMsg, oInnerException) {} // constructor
	} // class Warning
} // namespace
