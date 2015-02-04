namespace Ezbob.Utils {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Text;
	using JetBrains.Annotations;
	using Logger;

	public class TimeCounter {

		public TimeCounter(string sTitle) {
			m_sTitle = (sTitle ?? string.Empty).Trim();
			m_oCheckpoints = new List<Tuple<string, double>>();
		} // constructor

		[StringFormatMethod("sFormat")]
		public Timer AddStep(string sFormat, params object[] args) {
			return new Timer(m_oCheckpoints, sFormat, args);
		} // AddStep

		public class Timer : IDisposable {

			internal Timer(List<Tuple<string, double>> oCheckpoints, string sFormat, params object[] args) {
				m_oStopwatch = Stopwatch.StartNew();
				m_oCheckpoints = oCheckpoints;
				m_sCheckpoint = string.Format(sFormat, args);
			} // constructor

			public void Dispose() {
				m_oStopwatch.Stop();
				m_oCheckpoints.Add(new Tuple<string, double>(m_sCheckpoint, m_oStopwatch.Elapsed.TotalMilliseconds));
			} // Dispose

			private readonly Stopwatch m_oStopwatch;
			private readonly string m_sCheckpoint;
			private readonly List<Tuple<string, double>> m_oCheckpoints;

		} // class Timer

		public void Log(ASafeLog oLog, Severity nSeverity = Severity.Debug) {
			var sb = new StringBuilder();

			sb.AppendLine(m_sTitle);

			foreach (var time in m_oCheckpoints)
				sb.AppendFormat("\t{0}: {1}ms\n", time.Item1, time.Item2);

			oLog.Say(nSeverity, "{0}", sb);
		} // Log

		private readonly List<Tuple<string, double>> m_oCheckpoints;
		public List<Tuple<string, double>> Checkpoints { get { return m_oCheckpoints; } }
		public string Title { get { return m_sTitle; } }
		private readonly string m_sTitle;

	} // class TimeCounter
} // namespace
