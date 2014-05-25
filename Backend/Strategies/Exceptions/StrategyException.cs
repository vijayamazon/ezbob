namespace EzBob.Backend.Strategies.Exceptions {
	using System;

	/// <summary>
	/// Quiet exception. Writes nothing to log on creation.
	/// </summary>
	public class StrategyException : AStrategyException {
		public StrategyException(AStrategy oSource, string sMsg) : base(oSource, sMsg) {
		} // constructor

		public StrategyException(AStrategy oSource, string sMsg, Exception oInnerException) : base(oSource, sMsg, oInnerException) {
		} // constructor
	} // class StrategyException
} // namespace
