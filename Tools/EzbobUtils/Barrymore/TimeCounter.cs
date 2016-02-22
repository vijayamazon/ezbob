namespace Ezbob.Utils {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Text;
	using JetBrains.Annotations;

	public class TimeCounter {
		public TimeCounter(string sTitle = null) {
			Title = (sTitle ?? string.Empty).Trim();
			Steps = new List<Step>();
		} // constructor

		[StringFormatMethod("sFormat")]
		public Timer AddStep(string sFormat, params object[] args) {
			return new Timer(Steps, sFormat, args);
		} // AddStep

		public class Step {
			public Step(string name, double length) {
				Name = name;
				Length = length;
			} // constructor

			public string Name { get; private set; }
			public double Length { get; private set; }
		} // class Step

		public List<Step> Steps { get; private set; }

		public string Title { get; private set; }

		public TimeCounter Append(TimeCounter other) {
			if (other != null)
				Steps.AddRange(other.Steps);

			return this;
		} // Append

		public class Timer : IDisposable {
			internal Timer(List<Step> oCheckpoints, string sFormat, params object[] args) {
				m_oStopwatch = Stopwatch.StartNew();
				m_oCheckpoints = oCheckpoints;
				m_sCheckpoint = string.Format(sFormat, args);
			} // constructor

			public void Dispose() {
				m_oStopwatch.Stop();
				m_oCheckpoints.Add(new Step(m_sCheckpoint, m_oStopwatch.Elapsed.TotalMilliseconds));
			} // Dispose

			private readonly Stopwatch m_oStopwatch;
			private readonly string m_sCheckpoint;
			private readonly List<Step> m_oCheckpoints;
		} // class Timer

	    /// <summary>
	    /// Returns a string that represents the current object.
	    /// </summary>
	    /// <returns>
	    /// A string that represents the current object.
	    /// </returns>
	    public override string ToString() {
            var sb = new StringBuilder();
            sb.AppendLine(Title);
            foreach (var time in Steps)
                sb.AppendFormat("\t{0}: {1}ms\n", time.Name, time.Length);

            return sb.ToString();
        }//ToString
	} // class TimeCounter
} // namespace
