namespace Ezbob.Database {
	using System;
	using System.Data.SqlClient;
	using System.Threading;
	using Ezbob.Logger;

	public class SqlRetryer { 
		public SqlRetryer(int retryCount = 3, int sleepBeforeRetryMilliseconds = 0, ASafeLog log = null) {
			RetryCount = retryCount;
			SleepBeforeRetry = sleepBeforeRetryMilliseconds;
			Log = log.Safe();
			this.logVerbosityLevel = LogVerbosityLevel.Compact;
		} // constructor

		public virtual void Retry(Action oAction, string sFuncDescription = null) {
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
				} catch (SqlException e) {
					ex = e;

					switch (e.Number) {
					case ErrorNumber.Deadlock:
					case ErrorNumber.Locking:
					case ErrorNumber.UpdateConflict:
						sErrName = "Deadlock";
						break;

					case ErrorNumber.Timeout1:
					case ErrorNumber.Timeout2:
						sErrName = "Timeout";
						break;

					case ErrorNumber.Network:
						sErrName = "Network error";
						break;
					} // switch
				} // try

				if (sErrName == null)
					throw new DbException(ex.Message, ex);

				if (nCount < RetryCount) {
					Log.Warn(
						ex,
						"{2} encountered on attempt {0} of {1}, retrying after {3} milliseconds.",
						nCount,
						RetryCount,
						sErrName,
						SleepBeforeRetry
					);
					Thread.Sleep(SleepBeforeRetry);
				} else
					Log.Alert(
						ex,
						"{2} encountered on attempt {0} of {1}, out of retry attempts.",
						nCount,
						RetryCount,
						sErrName
					);
			} // for

			if (ex == null)
				throw new DbException("All the attempts failed, no further error information available.");

			throw new DbException("Out of retry attempts.", ex);
		} // Retry

		public virtual LogVerbosityLevel LogVerbosityLevel {
			get { return this.logVerbosityLevel; }
			set { this.logVerbosityLevel = value; }
		} // LogVerbosityLevel

		public virtual T Retry<T>(Func<T> func, string actionDescription = null) {
			if (func == null)
				throw new ArgumentNullException("func", "Function to retry not specified.");

			T res = default(T);

			Retry(() => { res = func(); }, actionDescription);

			return res;
		} // Retry

		public virtual int RetryCount {
			get { return this.retryCount; } // get
			private set { this.retryCount = (value < 1) ? 1 : value; } // set
		} // RetryCount

		/// <summary>
		/// In milliseconds.
		/// </summary>
		public virtual int SleepBeforeRetry {
			get { return this.sleepBeforeRetry; } // get
			private set { this.sleepBeforeRetry = (value < 0) ? 0 : value; } // set
		} // SleepBeforeRetry

		protected ASafeLog Log { get; private set; }

		private LogVerbosityLevel logVerbosityLevel;
		private int retryCount;
		private int sleepBeforeRetry;

		private static class ErrorNumber {
			public const int Network = 11;
			public const int Deadlock = 1205;
			public const int Locking = 1222;
			public const int UpdateConflict = 3960;
			public const int Timeout1 = -2;
			public const int Timeout2 = -2147217871;
		} // class ErrorNumber
	} // class SqlRetryer
} // namespace Ezbob.Database
