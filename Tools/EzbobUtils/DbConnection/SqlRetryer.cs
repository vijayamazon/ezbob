namespace Ezbob.Database {
	using System;
	using System.Data.SqlClient;
	using System.Threading;
	using Logger;
	using Utils;

	#region class SqlRetryer

	public class SqlRetryer : ARetryer {
		#region public

		#region constructor

		public SqlRetryer(int nRetryCount = 3, int nSleepBeforeRetry = 0, ASafeLog oLog = null) : base(nRetryCount, nSleepBeforeRetry, oLog) {
		} // constructor

		#endregion constructor

		#region method Retry

		public override T Retry<T>(Func<T> func, string sFuncDescription = null) {
			if (func == null)
				throw new ArgumentNullException("func", "Function to retry not specified.");

			Exception ex = null;

			sFuncDescription = string.IsNullOrWhiteSpace(sFuncDescription) ? "DB action" : sFuncDescription;

			for (int nCount = 1; nCount <= RetryCount; nCount++) {
				try {
					Log.Debug("Attempt {0} of {1}: {2}", nCount, RetryCount, sFuncDescription);

					T res = func();

					Log.Debug("Attempt {0} of {1} succeeded: {2}", nCount, RetryCount, sFuncDescription);

					return res;
				}
				catch (SqlException e) {
					ex = e;
					string sErrName;

					switch (e.Number) {
					case 1205:
						sErrName = "Deadlock";
						break;

					case -2:
					case -2147217871:
						sErrName = "Timeout";
						break;

					case 11:
						sErrName = "Network error";
						break;

					default:
						throw;
					} // switch

					if (nCount < RetryCount) {
						Log.Warn(e, "{2} encountered on attempt {0} of {1}, retrying after {3} seconds.", nCount, RetryCount, sErrName, SleepBeforeRetry);
						Thread.Sleep(TimeSpan.FromSeconds(SleepBeforeRetry));
					}
					else
						Log.Alert(e, "{2} encountered on attempt {0} of {1}, out of retry attempts.", nCount, RetryCount, sErrName);
				} // try
			} // for

			if (ex == null)
				throw new Exception("All the attempts failed, no further error information available.");

			throw ex;
		} // Retry

		#endregion method Retry

		#endregion public
	} // class SqlRetryer

	#endregion class SqlRetryer
} // namespace Ezbob.Database
