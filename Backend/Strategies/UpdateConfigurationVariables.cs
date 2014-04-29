namespace EzBob.Backend.Strategies {
	using Ezbob.Database;
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

			lock (updateConfigurationVariablesLock)
			{
				isExecuting = false;
			} // lock
		} // Execute



		private static readonly object updateConfigurationVariablesLock = new object();
		private static bool isExecuting;
	} // UpdateCurrencyRates
} // namespace
