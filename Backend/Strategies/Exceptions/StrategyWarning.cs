namespace Ezbob.Backend.Strategies.Exceptions {
	using System;
	using Ezbob.Utils.Exceptions;

	public class StrategyWarning : Warning {
		public StrategyWarning(
			AStrategy oSource,
			string sMsg,
			Exception oInnerException = null
		) : base(oSource.Log, StrategyException.Msg(oSource, sMsg), oInnerException) {} // constructor
	} // class StrategyWarning
} // namespace
