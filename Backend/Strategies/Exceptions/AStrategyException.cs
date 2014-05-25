namespace EzBob.Backend.Strategies.Exceptions {
	using System;

	/// <summary>
	/// Base exception. All the non-standard exceptions thrown from Service should inherit from this one.
	/// This exception must be inherited.
	/// </summary>
	public abstract class AStrategyException : Exception {
		protected AStrategyException(AStrategy oSource, string sMsg) : base(oSource.Name + ": " + sMsg) {
		} // constructor

		protected AStrategyException(AStrategy oSource, string sMsg, Exception oInnerException) : base(oSource.Name + ": " + sMsg, oInnerException) {
		} // constructor
	} // class AStrategyException
} // namespace EzBob.Backend.Strategies.Exceptions
