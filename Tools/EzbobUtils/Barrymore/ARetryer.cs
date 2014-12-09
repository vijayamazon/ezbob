namespace Ezbob.Utils {
	using System;
	using Logger;

	public class ForceRetryException : Exception {
		public const string Name = "Force retry";

		public ForceRetryException(string sMsg = null, Exception oInner = null) : base(sMsg ?? Name, oInner) {} // constructor
	} // class ForceRetryException

	public abstract class ARetryer {

		public virtual LogVerbosityLevel LogVerbosityLevel {
			get { return m_nLogVerbosityLevel; }
			set { m_nLogVerbosityLevel = value; }
		} // LogVerbosityLevel

		private LogVerbosityLevel m_nLogVerbosityLevel;

		protected ARetryer(int nRetryCount = 1, int nSleepBeforeRetryMilliseconds = 0, ASafeLog oLog = null) {
			RetryCount = nRetryCount;
			SleepBeforeRetry = nSleepBeforeRetryMilliseconds;
			Log = new SafeLog(oLog);
			m_nLogVerbosityLevel = LogVerbosityLevel.Compact;
		} // constructor

		protected virtual int RetryCount {
			get { return m_nRetryCount; } // get
			private set { m_nRetryCount = (value < 1) ? 1 : value; } // set
		} // RetryCount

		private int m_nRetryCount;

		/// <summary>
		/// In milliseconds.
		/// </summary>
		protected virtual int SleepBeforeRetry {
			get { return m_nSleepBeforeRetry; } // get
			private set { m_nSleepBeforeRetry = (value < 0) ? 0 : value; } // set
		} // SleepBeforeRetry

		private int m_nSleepBeforeRetry;

		public virtual T Retry<T>(Func<T> oFunc, string sActionDescription = null) {
			if (oFunc == null)
				throw new ArgumentNullException("oFunc", "Function to retry not specified.");

			T res = default(T);

			Retry(() => { res = oFunc(); }, sActionDescription);

			return res;
		} // Retry

		public abstract void Retry(Action func, string sFuncDescription = null); // Retry

		protected ASafeLog Log { get; private set; }

	} // class ARetryer

} // namespace
