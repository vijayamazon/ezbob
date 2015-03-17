namespace Ezbob.Backend.Strategies {
	using System;
	using Ezbob.Logger;

	public class StrategyLog : ASafeLog {
		public StrategyLog(AStrategy oStrategy, ASafeLog oLog) : base(null) {
			if (ReferenceEquals(oStrategy, null))
				throw new Exception("Cannot initialize logger", new ArgumentNullException("oStrategy"));

			m_oStrategy = oStrategy;
			m_oLog = new SafeLog(oLog);
		} // constructor

		protected override void OwnSay(Severity nSeverity, string format, params object[] parameters) {
			m_oLog.Say(nSeverity, m_oStrategy.Name + " strategy: " + format, parameters);
		} // OwnSay

		protected override void OwnSay(Severity nSeverity, Exception ex, string format, params object[] parameters) {
			m_oLog.Say(nSeverity, ex, m_oStrategy.Name + " strategy: " + format, parameters);
		} // OwnSay

		private readonly AStrategy m_oStrategy;
		private readonly SafeLog m_oLog;
	} // class StrategyLog
} // namespace Ezbob.Backend.Strategies
