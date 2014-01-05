namespace EzBob.Backend.Strategies {
	using System;
	using Ezbob.Logger;

	#region class StrategyLog

	public class StrategyLog : ASafeLog {
		#region public

		#region constructor

		public StrategyLog(AStrategy oStrategy, ASafeLog oLog) : base(null) {
			if (ReferenceEquals(oStrategy, null))
				throw new Exception("Cannot initialise logger", new ArgumentNullException("oStrategy"));

			m_oStrategy = oStrategy;
			m_oLog = new SafeLog(oLog);
		} // constructor

		#endregion constructor

		#endregion public

		#region protected

		#region method OwnSay

		protected override void OwnSay(Severity nSeverity, string format, params object[] parameters) {
			m_oLog.Say(nSeverity, m_oStrategy.Name + " strategy: " + format, parameters);
		} // OwnSay

		protected override void OwnSay(Severity nSeverity, Exception ex, string format, params object[] parameters) {
			m_oLog.Say(nSeverity, ex, m_oStrategy.Name + " strategy: " + format, parameters);
		} // OwnSay

		#endregion method OwnSay

		#endregion protected

		#region private

		private readonly AStrategy m_oStrategy;
		private readonly SafeLog m_oLog;

		#endregion private
	} // class StrategyLog

	#endregion class StrategyLog
} // namespace EzBob.Backend.Strategies
