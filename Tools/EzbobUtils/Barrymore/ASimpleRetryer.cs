namespace Ezbob.Utils {
	using System;
	using System.Collections.Generic;
	using System.Threading;
	using Logger;

	public abstract class ASimpleRetryer {
		public virtual LogVerbosityLevel LogVerbosityLevel {
			get { return this.logVerbosityLevel; }
			set { this.logVerbosityLevel = value; }
		} // LogVerbosityLevel

		public virtual void Execute() {
			string funcDescription = (
				string.IsNullOrWhiteSpace(ActionDescription) ? "action to retry" : ActionDescription
			).Trim();

			Exception ex = null;

			if (OnStart != null)
				OnStart();

			// Must be declared here to preserve the last value in the SleepBetweenRetires queue (if queue is shorter
			// than number of retries). The queue should contain at least one value because it is inserted in the
			// constructor.
			uint sleepBeforeRetry = 0;

			for (int attemptNo = 1; attemptNo <= RetryCount; attemptNo++) {
				ex = null;

				if (OnBeforeRetry != null)
					OnBeforeRetry(attemptNo);

				if (LogVerbosityLevel == LogVerbosityLevel.Verbose)
					Log.Debug("Attempt {0} out of {1} executing {2}...", attemptNo, RetryCount, funcDescription);

				ActionOutcomes outcome = ActionOutcomes.NotExecuted;

				try {
					outcome = ActionToRetry(attemptNo);
				} catch (Exception e) {
					ex = e;
				} // try

				if (LogVerbosityLevel == LogVerbosityLevel.Verbose) {
					Log.Say(
						ex == null ? Severity.Debug : Severity.Warn,
						ex,
						"Attempt {0} out of {1} executing {2} completed with result '{3}'.",
						attemptNo,
						RetryCount,
						funcDescription,
						outcome
					);
				} // if

				if (OnAfterRetry != null)
					OnAfterRetry(attemptNo, outcome, ex);

				if (outcome == ActionOutcomes.Done) {
					if (LogVerbosityLevel == LogVerbosityLevel.Verbose) {
						Log.Msg(
							"Attempt {0} out of {1} executing {2} was successful, done with retry attempts.",
							attemptNo,
							RetryCount,
							funcDescription
						);
					} // if

					if (OnSuccess != null)
						OnSuccess(attemptNo);

					return;
				} // if

				if (outcome == ActionOutcomes.Fatal) {
					if (LogVerbosityLevel == LogVerbosityLevel.Verbose) {
						Log.Warn(
							"Attempt {0} out of {1} executing {2} completed with fatal error, done with retry attempts.",
							attemptNo,
							RetryCount,
							funcDescription
						);
					} // if

					if (OnFail != null)
						OnFail(attemptNo, outcome, ex);

					return;
				} // if

				if (attemptNo < RetryCount) {
					if (SleepBetweenRetries.Count > 0)
						sleepBeforeRetry = SleepBetweenRetries.Dequeue();

					if (LogVerbosityLevel == LogVerbosityLevel.Verbose) {
						Log.Msg(
							"Attempt {0} out of {1} executing {2} was not successful, retrying after {3} milliseconds.",
							attemptNo,
							RetryCount,
							funcDescription,
							sleepBeforeRetry
						);
					} // if

					Thread.Sleep(sleepBeforeRetry <= Int32.MaxValue ? (int)sleepBeforeRetry : Int32.MaxValue);
				} else {
					if (LogVerbosityLevel == LogVerbosityLevel.Verbose) {
						Log.Warn(
							"Attempt {0} out of {1} executing {2} was not successful, out of retry attempts.",
							attemptNo,
							RetryCount,
							funcDescription
						);
					} // if
				} // if
			} // for

			if (OnFail != null)
				OnFail(RetryCount, ActionOutcomes.Retry, ex);
		} // Retry

		protected ASimpleRetryer(int retryCount, uint sleepBeforeRetryMilliseconds, ASafeLog log) {
			RetryCount = retryCount;
			Log = log.Safe();

			this.logVerbosityLevel = LogVerbosityLevel.Compact;

			SleepBetweenRetries = new Queue<uint>();
			SleepBetweenRetries.Enqueue(sleepBeforeRetryMilliseconds);
		} // constructor

		protected Queue<uint> SleepBetweenRetries { get; private set; }

		protected enum ActionOutcomes {
			/// <summary>
			/// Attempt was not executed.
			/// </summary>
			NotExecuted,

			/// <summary>
			/// No further attempts needed.
			/// </summary>
			Done,

			/// <summary>
			/// Further attempt is needed.
			/// </summary>
			Retry,

			/// <summary>
			/// Abort mission, further attempts cannot be executed.
			/// </summary>
			Fatal,
		} // enum ActionOutcomes

		/// <summary>
		/// Executes action that should be retried on failure.
		/// </summary>
		/// <returns>Action outcome: continue or not with attempts.</returns>
		protected abstract ActionOutcomes ActionToRetry(int attemptNo);

		/// <summary>
		/// Short action to retry description for logging purposes.
		/// </summary>
		protected abstract string ActionDescription { get; }

		protected int RetryCount { get; private set; }
		protected ASafeLog Log { get; private set; }

		protected delegate void ParameterlessEventHandler();

		protected delegate void CompleteEventHandler(int nRetryIndex);

		protected delegate void BeforeRetryEventHandler(int nRetryIndex);

		protected delegate void AfterRetryEventHandler(int nRetryIndex, ActionOutcomes outcome, Exception e);

		protected event ParameterlessEventHandler OnStart;

		protected event CompleteEventHandler OnSuccess;
		protected event AfterRetryEventHandler OnFail;

		protected event BeforeRetryEventHandler OnBeforeRetry;
		protected event AfterRetryEventHandler OnAfterRetry;

		private LogVerbosityLevel logVerbosityLevel;
	} // class ASimpleRetryer
} // namespace
