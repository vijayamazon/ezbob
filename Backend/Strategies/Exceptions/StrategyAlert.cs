namespace EzBob.Backend.Strategies.Exceptions {
	using System;
	using Ezbob.Logger;

	/// <summary>
	/// Writes an ALERT (ERROR) to log on creation.
	/// </summary>
	public class StrategyAlert : AStrategyLoggedException {
		public StrategyAlert(AStrategy oSource, string sMsg, Exception oInnerException = null)
			: base(Severity.Alert, oSource, sMsg, oInnerException)
		{} // constructor
	} // class StrategyException
} // namespace
