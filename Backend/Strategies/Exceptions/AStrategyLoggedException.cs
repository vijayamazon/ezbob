namespace EzBob.Backend.Strategies.Exceptions {
	using System;
	using Ezbob.Logger;

	/// <summary>
	/// Writes a message with requested severity to log on creation.
	/// </summary>
	public abstract class AStrategyLoggedException : AStrategyException {
		protected AStrategyLoggedException(Severity nSeverity, AStrategy oSource, string sMsg, Exception oInnerException = null) : base(oSource, sMsg, oInnerException) {
			oSource.Log.Say(nSeverity, this);
		} // constructor
	} // class AStrategyLoggedException
} // namespace
