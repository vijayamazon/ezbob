namespace EzService.Exceptions {
	using System;
	using EzBob.Backend.Strategies.Exceptions;
	using Ezbob.Logger;

	/// <summary>
	/// Writes a WARN to log on creation.
	/// </summary>
	public class ServiceWarning : AStrategyLoggedException {
		public ServiceWarning(string sMsg, Exception oInnerException = null) : base(Severity.Warn, null, sMsg, oInnerException) {} // constructor
	} // class ServiceWarning
} // namespace
