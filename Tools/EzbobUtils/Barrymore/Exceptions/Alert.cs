namespace Ezbob.Utils.Exceptions {
	using System;
	using Ezbob.Logger;

	public class Alert : ALoggedException {
		public Alert(ASafeLog oLog, string sMsg, Exception oInnerException = null) : base(Severity.Alert, oLog, sMsg, oInnerException) {} // constructor
	} // class Alert
} // namespace
