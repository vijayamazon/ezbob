namespace Ezbob.Backend.Strategies.Misc {
	using System;
	using CustomSchedulers.Currency;

	public class UpdateCurrencyRates : AStrategy {
		public override string Name {
			get { return "Update currency rates"; }
		} // Name

		public override void Execute() {
			lock (updateCurrencyRatesLock) {
				if (isExecuting) {
					Log.Warn("Update currency rates is already in progress.");
					return;
				} // if

				isExecuting = true;
			} // lock

			try {
				CurrencyUpdateController.Run();
			} catch (Exception ex) {
				Log.Error(ex, "UpdateCurrencyRates failed");
			}
			lock (updateCurrencyRatesLock) {
				isExecuting = false;
			} // lock
		} // Execute

		private static readonly object updateCurrencyRatesLock = new object();
		private static bool isExecuting;
	} // UpdateCurrencyRates
} // namespace
