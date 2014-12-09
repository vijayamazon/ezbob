namespace Ezbob.Backend.Strategies.Exceptions {
	using System;
	using Ezbob.Utils.Exceptions;

	public class StrategyException : QuietException {
		public StrategyException(
			AStrategy oSource,
			string sMsg,
			Exception oInnerException = null
		) : base(StrategyException.Msg(oSource, sMsg), oInnerException) {} // constructor

		public static string Msg(AStrategy oSource, string sMsg) {
			return
				(oSource == null ? string.Empty : oSource.Name + ": ") +
				(string.IsNullOrWhiteSpace(sMsg) ? "Something exceptional happened" : sMsg);
		} // Msg
	} // class StrategyException
} // namespace
