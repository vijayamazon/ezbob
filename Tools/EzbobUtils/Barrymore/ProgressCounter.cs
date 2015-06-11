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
			return pc.Increment();
		} // oprator ++

		public ProgressCounter Increment() {
			++this.m_nCounter;

			if (this.m_nCounter == this.m_nCheckpoint) {
				this.m_n1k += this.m_nCounter;
				this.m_nCounter = 0;

				Say(this.m_nSeverity, this.m_sFormat, this.m_n1k);
			} // if

			return this;
		} // Increment

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
