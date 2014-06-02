namespace EzService.Exceptions {
	using System;
	using EzBob.Backend.Strategies.Exceptions;
	using Ezbob.Logger;

	/// <summary>
	/// Writes an ALERT (ERROR) to log on creation.
	/// </summary>
	public class ServiceAlert : AStrategyLoggedException {
		public ServiceAlert(string sMsg, Exception oInnerException = null) : base(Severity.Alert, null, sMsg, oInnerException) {} // constructor
	} // class ServiceAlert
} // namespace
