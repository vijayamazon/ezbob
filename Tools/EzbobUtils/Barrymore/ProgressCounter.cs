namespace Ezbob.Utils {
	using Ezbob.Logger;

	public class ProgressCounter : SafeLog {

		public ProgressCounter(string sFormat, ASafeLog oLog = null, ulong nCheckpoint = 1000, Severity nSeverity = 0) : base(oLog) {
			m_nCounter = 0;
			m_n1k = 0;
			m_nCheckpoint = nCheckpoint;
			m_nSeverity = nSeverity;
			m_sFormat = sFormat ?? "";
		} // constructor

		public static ProgressCounter operator ++(ProgressCounter pc) {
			++pc.m_nCounter;

			if (pc.m_nCounter == pc.m_nCheckpoint) {
				pc.m_n1k += pc.m_nCounter;
				pc.m_nCounter = 0;

				pc.Say(pc.m_nSeverity, pc.m_sFormat, pc.m_n1k);
			} // if

			return pc;
		} // oprator ++

		public void Log() {
			if (m_nCounter == 0) {
				if (m_n1k == 0)
					Say(m_nSeverity, m_sFormat, 0);
			}
			else
				Say(m_nSeverity, m_sFormat, m_n1k + m_nCounter);
		} // Log

		private ulong m_nCounter;
		private ulong m_n1k;
		private readonly ulong m_nCheckpoint;
		private readonly Severity m_nSeverity;
		private readonly string m_sFormat;

	} // class ProgressCounter
} // namespace
