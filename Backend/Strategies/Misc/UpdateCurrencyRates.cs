namespace EzBob.Backend.Strategies.Misc {
	using CustomSchedulers.Currency;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class UpdateCurrencyRates : AStrategy {
		public UpdateCurrencyRates(AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog) {
		}

		public override string Name {
			get { return "Update currency rates"; }
		} // Name

		public override void Execute() {
			lock (updateCurrencyRatesLock) {
				if (isExecuting)
				{
					Log.Warn("Update currency rates is already in progress.");
					return;
				} // if

				isExecuting = true;
			} // lock

			CurrencyUpdateController.Run();

			lock (updateCurrencyRatesLock)
			{
				isExecuting = false;
			} // lock
		} // Execute

		private static readonly object updateCurrencyRatesLock = new object();
		private static bool isExecuting;
	} // UpdateCurrencyRates
} // namespace
