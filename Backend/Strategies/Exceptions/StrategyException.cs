using System;

namespace EzBob.Backend.Strategies.Exceptions
{
	#region class StrategyException

	public class StrategyException : Exception {
		#region public

		#region constructor

		public StrategyException(AStrategy oSource, string sMsg) : base(oSource.Name + ": " + sMsg) {
		} // constructor

		public StrategyException(AStrategy oSource, string sMsg, Exception oInnerException) : base(oSource.Name + ": " + sMsg, oInnerException) {
		} // constructor

		#endregion constructor

		#endregion public
	} // class StrategyException

	#endregion class StrategyException
} // namespace EzBob.Backend.Strategies
