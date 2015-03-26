namespace Ezbob.Utils.Exceptions {
	using System;
	using Ezbob.Logger;

	public abstract class ALoggedException : AException {
		protected ALoggedException(Severity nSeverity, ASafeLog oLog, string sMsg, Exception oInnerException = null) : base(sMsg, oInnerException) {
			if (oLog != null)
				oLog.Say(nSeverity, this, "{0}", sMsg);
		} // constructor
	} // class ALoggedException
} // namespace
