namespace Ezbob.Backend.Strategies.Exceptions {
	using System;
	using Ezbob.Utils.Exceptions;

	public class StrategyAlert : Alert {
		public StrategyAlert(
			AStrategy oSource,
			string sMsg,
			Exception oInnerException = null
		) : base(oSource.Log, StrategyException.Msg(oSource, sMsg), oInnerException) {} // constructor
	} // class StrategyException
} // namespace
