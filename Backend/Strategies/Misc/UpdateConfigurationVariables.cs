namespace EzBob.Backend.Strategies.Misc {
	using ConfigManager;
	using Ezbob.Database;
	using Ezbob.Database.Pool;
	using Ezbob.Logger;

	public class UpdateConfigurationVariables : AStrategy {
		public UpdateConfigurationVariables(AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog) {
		}

		public override string Name {
			get { return "Update configuration variables"; }
		} // Name

		public override void Execute() {
			lock (updateConfigurationVariablesLock)
			{
				if (isExecuting)
				{
					Log.Warn("Update configuration variables is already in progress.");
					return;
				} // if

				isExecuting = true;
			} // lock

			ConfigManager.CurrentValues.ReInit();
			DbConnectionPool.ReuseCount = CurrentValues.Instance.ConnectionPoolReuseCount;
			AConnection.UpdateConnectionPoolMaxSize(CurrentValues.Instance.ConnectionPoolMaxSize);

			lock (updateConfigurationVariablesLock)
			{
				isExecuting = false;
			} // lock
		} // Execute



		private static readonly object updateConfigurationVariablesLock = new object();
		private static bool isExecuting;
	} // UpdateCurrencyRates
} // namespace
