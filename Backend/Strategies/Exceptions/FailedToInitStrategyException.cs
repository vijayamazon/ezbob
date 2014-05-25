using System;

namespace EzBob.Backend.Strategies.Exceptions
{
	#region class StrategyException

	public class FailedToInitStrategyException : StrategyException {
		#region public

		#region constructor

		public FailedToInitStrategyException(AStrategy oSource, string sMsg) : base(oSource, sMsg) {
		} // constructor

		public FailedToInitStrategyException(AStrategy oSource, string sMsg, Exception oInnerException) : base(oSource, sMsg, oInnerException) {
		} // constructor

		public FailedToInitStrategyException(AStrategy oSource, Exception oInnerException) : base(oSource, "failed to initialise", oInnerException) {
		} // constructor

		#endregion constructor

		#endregion public
	} // class StrategyException

	#endregion class StrategyException
} // namespace EzBob.Backend.Strategies
