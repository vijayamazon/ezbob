namespace EzBob.Backend.Strategies.Exceptions {
	using System;

	public class FailedToInitStrategyException : StrategyAlert {
		public FailedToInitStrategyException(AStrategy oSource, Exception oInnerException) : base(oSource, "Failed to initialise.", oInnerException) {
		} // constructor
	} // class StrategyException
} // namespace EzBob.Backend.Strategies
