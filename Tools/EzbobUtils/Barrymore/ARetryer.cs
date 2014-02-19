﻿namespace Ezbob.Utils {
	using System;
	using Logger;

	#region class ARetryer

	public abstract class ARetryer {
		#region protected

		#region constructor

		protected ARetryer(int nRetryCount = 1, int nSleepBeforeRetry = 0, ASafeLog oLog = null) {
			RetryCount = nRetryCount;
			SleepBeforeRetry = nSleepBeforeRetry;
			Log = new SafeLog(oLog);
		} // constructor

		#endregion constructor

		#region property RetryCount

		protected virtual int RetryCount {
			get { return m_nRetryCount; } // get
			private set { m_nRetryCount = (value < 1) ? 1 : value; } // set
		} // RetryCount

		private int m_nRetryCount;

		#endregion property RetryCount

		#region property SleepBeforeRetry

		protected virtual int SleepBeforeRetry {
			get { return m_nSleepBeforeRetry; } // get
			private set { m_nSleepBeforeRetry = (value < 0) ? 0 : value; } // set
		} // SleepBeforeRetry

		private int m_nSleepBeforeRetry;

		#endregion property SleepBeforeRetry

		#region method Retry

		public virtual void Retry(Action oAction, string sActionDescription = null) {
			Retry<object>(() => {
				oAction();
				return null;
			}, sActionDescription);
		} // Retry

		public abstract T Retry<T>(Func<T> func, string sFuncDescription = null); // Retry

		#endregion method Retry

		#region property Log

		protected ASafeLog Log { get; private set; }

		#endregion property Log

		#endregion protected
	} // class ARetryer

	#endregion class ARetryer
} // namespace Ezbob.Utils
