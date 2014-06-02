namespace EzBob.Backend.Strategies.Exceptions {
	using System;
	using Ezbob.Logger;

	/// <summary>
	/// Writes a WARN to log on creation.
	/// </summary>
	public class StrategyWarning : AStrategyLoggedException {
		public StrategyWarning(AStrategy oSource, string sMsg, Exception oInnerException = null) : base(Severity.Warn, oSource, sMsg, oInnerException) {} // constructor
	} // class StrategyWarning
} // namespace
