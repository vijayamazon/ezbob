namespace EzBob.Backend.Strategies {
	using System;
	using Exceptions;
	using Ezbob.Database;
	using Ezbob.Logger;

	#region class AStrategy

	public abstract class AStrategy {
		#region public

		public abstract string Name { get; }

		public abstract void Execute();

		public AConnection DB { get; private set; }
		public StrategyLog Log { get; private set; }

		#endregion public

		#region protected

		protected AStrategy(AConnection oDB, ASafeLog oLog) {
			if (ReferenceEquals(oDB, null))
				throw new FailedToInitStrategyException(this, new ArgumentNullException("oDB", "DB connection is not specified for mail strategy."));

			DB = oDB;
			Log = new StrategyLog(this, oLog);
		} // constructor

		#endregion protected
	} // class AStrategy

	#endregion class AStrategy
} // namespace EzBob.Backend.Strategies
