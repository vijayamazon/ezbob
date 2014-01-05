namespace EzBob.Backend.Strategies {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	#region class AStrategy

	public abstract class AStrategy {
		#region public

		public abstract string Name { get; }

		public abstract void Execute();

		#endregion public

		#region protected

		protected AStrategy(AConnection oDb, ASafeLog oLog) {
			if (ReferenceEquals(oDb, null))
				throw new FailedToInitStrategyException(this, new ArgumentNullException("oDb", "DB connection is not specified for mail strategy."));

			DB = oDb;
			Log = new StrategyLog(this, oLog);
		} // constructor

		protected AConnection DB { get; private set; }
		protected StrategyLog Log { get; private set; }

		#endregion protected
	} // class AStrategy

	#endregion class AStrategy
} // namespace EzBob.Backend.Strategies
