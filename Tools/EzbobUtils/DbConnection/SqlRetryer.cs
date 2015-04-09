namespace Ezbob.Database {
	using System;
	using System.Data.SqlClient;
	using System.Threading;
	using Ezbob.Logger;
	using Ezbob.Utils;

	public class SqlRetryer : ARetryer {
		public SqlRetryer(
			int nRetryCount = 3,
			int nSleepBeforeRetryMilliseconds = 0,
			ASafeLog oLog = null
		) : base(nRetryCount, nSleepBeforeRetryMilliseconds, oLog) {
		} // constructor

		public override void Retry(Action oAction, string sFuncDescription = null) {
			if (oAction == null)
				throw new ArgumentNullException("oAction", "Function to retry not specified.");

			Exception ex = null;

			sFuncDescription = string.IsNullOrWhiteSpace(sFuncDescription) ? "DB action" : sFuncDescription;

			for (int nCount = 1; nCount <= RetryCount; nCount++) {
				string sErrName = null;

				try {
					ex = null;

					if (LogVerbosityLevel == LogVerbosityLevel.Verbose)
						Log.Debug("Attempt {0} of {1}: {2}", nCount, RetryCount, sFuncDescription);

					oAction();

					if (LogVerbosityLevel == LogVerbosityLevel.Verbose)
						Log.Debug("Success on attempt {0} of {1}: {2}", nCount, RetryCount, sFuncDescription);

					return;
				} catch (ForceRetryException e) {
					ex = e;
					sErrName = ForceRetryException.Name;
				} catch (SqlException e) {
					ex = e;
					sErrName = null;

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
					} // switch
				} // try

				if ((sErrName == null) && (ex != null))
					throw new DbException(ex.Message, ex);

				if (nCount < RetryCount) {
					Log.Warn(ex, "{2} encountered on attempt {0} of {1}, retrying after {3} milliseconds.", nCount, RetryCount, sErrName, SleepBeforeRetry);
					Thread.Sleep(SleepBeforeRetry);
				} else
					Log.Alert(ex, "{2} encountered on attempt {0} of {1}, out of retry attempts.", nCount, RetryCount, sErrName);
			} // for

			if (ex == null)
				throw new DbException("All the attempts failed, no further error information available.");

			throw new DbException("Out of retry attempts.", ex);
		} // Retry
	} // class SqlRetryer
} // namespace Ezbob.Database
