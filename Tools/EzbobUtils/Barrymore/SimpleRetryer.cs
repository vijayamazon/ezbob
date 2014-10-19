namespace Ezbob.Utils {
	using System;
	using System.Threading;
	using Logger;

	public class SimpleRetryer : ARetryer {
		#region constructor

		public SimpleRetryer(int nRetryCount = 3, int nSleepBeforeRetryMilliseconds = 0, ASafeLog oLog = null)
			: base(nRetryCount, nSleepBeforeRetryMilliseconds, oLog) {
		} // constructor

		#endregion constructor

		public delegate void ParameterlessEventHandler();

		public delegate void CompleteEventHandler(int nRetryIndex);

		public delegate void BeforeRetryEventHandler(int nRetryIndex, bool bIsFirst, bool bIsLast);

		public delegate void AfterRetryEventHandler(int nRetryIndex, bool bIsFirst, bool bIsLast, bool bSuccess);

		public event ParameterlessEventHandler OnStart;

		public event CompleteEventHandler OnSuccess;
		public event CompleteEventHandler OnFail;

		public event BeforeRetryEventHandler OnBeforeRetry;
		public event AfterRetryEventHandler OnAfterRetry;

		#region method Retry

		public override void Retry(Action oAction, string sFuncDescription = null) {
			if (oAction == null)
				throw new ArgumentNullException("oAction", "Action to retry not specified.");

			Exception ex = null;

			bool bSuccess = false;

			if (OnStart != null)
				OnStart();

			for (int nCount = 1; nCount <= RetryCount; nCount++) {
				if (OnBeforeRetry != null)
					OnBeforeRetry(nCount, nCount == 1, nCount == RetryCount);

				try {
					if (LogVerbosityLevel == LogVerbosityLevel.Verbose)
						Log.Debug("Attempt {0} of {1}: {2}", nCount, RetryCount, sFuncDescription);

					oAction();

					if (LogVerbosityLevel == LogVerbosityLevel.Verbose)
						Log.Debug("Success on attempt {0} of {1}: {2}", nCount, RetryCount, sFuncDescription);

					bSuccess = true;
				}
				catch (Exception e) {
					ex = e;
				} // try

				if (OnAfterRetry != null)
					OnAfterRetry(nCount, nCount == 1, nCount == RetryCount, bSuccess);

				if (bSuccess) {
					if (OnSuccess != null)
						OnSuccess(nCount);

					return;
				} // if

				if (nCount < RetryCount) {
					Log.Warn(ex, "An error encountered on attempt {0} of {1}, retrying after {2} milliseconds.", nCount, RetryCount, SleepBeforeRetry);
					Thread.Sleep(SleepBeforeRetry);
				}
				else
					Log.Alert(ex, "An error encountered on attempt {0} of {1}, out of retry attempts.", nCount, RetryCount);
			} // for

			if (OnFail != null)
				OnFail(RetryCount);

			if (ex == null)
				throw new Exception("All the attempts failed, no further error information available.");

			throw new Exception("Out of retry attempts.", ex);
		} // Retry

		#endregion method Retry
	} // class SimpleRetryer
} // namespace
