namespace SalesForceLib {
	using System;
	using System.Threading;
	using log4net;

	public static class SalesForceRetier {
		private static readonly ILog Log = LogManager.GetLogger(typeof(SalesForceRetier));

		public static void Execute(int numberOfRetries, int retryWaitSeconds, ISalesForceAppClient client, Action action) {
			do {
				action.Invoke();

				if (client.HasError) {
					Log.WarnFormat("SalesForce error occurred waiting {0} seconds, number of retries left {1}", retryWaitSeconds, numberOfRetries - 1);
					Thread.Sleep(retryWaitSeconds * 1000);
					if (client.HasLoginError) {
						Log.Warn("SalesForce login to partners service error occurred, re-login");
						client.Login();
					}
					numberOfRetries--;
				} else {
					break;
				}
			} while (numberOfRetries >= 1);
		}
	}
}
