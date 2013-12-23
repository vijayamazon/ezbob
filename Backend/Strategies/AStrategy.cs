using System;
using Ezbob.Database;
using Ezbob.Logger;

namespace EzBob.Backend.Strategies {
	#region class AStrategy

	public abstract class AStrategy {
		#region public

		public abstract string Name { get; }

		public abstract void Execute();

		#endregion public

		#region protected

		protected AStrategy(AConnection oDB, ASafeLog oLog) {
			if (ReferenceEquals(oDB, null))
				throw new FailedToInitStrategyException(this, new ArgumentNullException("oDB", "DB connection is not specified for mail strategy."));

			DB = oDB;
			Log = new StrategyLog(this, oLog);
		} // constructor

		protected AConnection DB { get; private set; }
		protected StrategyLog Log { get; private set; }

		#endregion protected
	} // class AStrategy

	#endregion class AStrategy
} // namespace EzBob.Backend.Strategies
