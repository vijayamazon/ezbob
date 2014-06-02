namespace EzBob.Backend.Strategies.Exceptions {
	using System;

	/// <summary>
	/// Base exception. All the non-standard exceptions thrown from Service should inherit from this one.
	/// This exception must be inherited.
	/// </summary>
	public abstract class AStrategyException : Exception {
		protected AStrategyException(AStrategy oSource, string sMsg) : base(GetStrategyName(oSource) + ": " + sMsg) {
		} // constructor

		protected AStrategyException(AStrategy oSource, string sMsg, Exception oInnerException) : base(GetStrategyName(oSource) + ": " + sMsg, oInnerException) {
		} // constructor

		private static string GetStrategyName(AStrategy oSource) {
			return oSource == null ? string.Empty : oSource.Name;
		} // GetStrategyName
	} // class AStrategyException
} // namespace EzBob.Backend.Strategies.Exceptions
