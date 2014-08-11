namespace Ezbob.Utils {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Text;
	using JetBrains.Annotations;
	using Logger;

	public class TimeCounter {
		#region public

		#region constructor

		public TimeCounter(string sTitle) {
			m_sTitle = (sTitle ?? string.Empty).Trim();
			m_oCheckpoints = new List<Tuple<string, double>>();
		} // constructor

		#endregion constructor

		#region method AddStep

		[StringFormatMethod("sFormat")]
		public Timer AddStep(string sFormat, params object[] args) {
			return new Timer(m_oCheckpoints, sFormat, args);
		} // AddStep

		#endregion method AddStep

		#region class Timer

		public class Timer : IDisposable {
			#region constructor

			internal Timer(List<Tuple<string, double>> oCheckpoints, string sFormat, params object[] args) {
				m_oStopwatch = Stopwatch.StartNew();
				m_oCheckpoints = oCheckpoints;
				m_sCheckpoint = string.Format(sFormat, args);
			} // constructor

			#endregion constructor

			#region method Dispose

			public void Dispose() {
				m_oStopwatch.Stop();
				m_oCheckpoints.Add(new Tuple<string, double>(m_sCheckpoint, m_oStopwatch.Elapsed.TotalMilliseconds));
			} // Dispose

			#endregion method Dispose

			#region private

			private readonly Stopwatch m_oStopwatch;
			private readonly string m_sCheckpoint;
			private readonly List<Tuple<string, double>> m_oCheckpoints;

			#endregion private
		} // class Timer

		#endregion class Timer

		#region method Log

		public void Log(ASafeLog oLog, Severity nSeverity = Severity.Debug) {
			var sb = new StringBuilder();

			sb.AppendLine(m_sTitle);

			foreach (var time in m_oCheckpoints)
				sb.AppendFormat("\t{0}: {1}ms\n", time.Item1, time.Item2);

			oLog.Say(nSeverity, "{0}", sb);
		} // Log

		#endregion method LogElapsedTimes

		#endregion public

		#region private

		private readonly List<Tuple<string, double>> m_oCheckpoints;
		private readonly string m_sTitle;

		#endregion private
	} // class TimeCounter
} // namespace
