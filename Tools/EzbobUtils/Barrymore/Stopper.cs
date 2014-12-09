namespace Ezbob.Utils {
	using System;
	using System.Diagnostics;

	public class Stopper {

		public Stopper() {
			m_oStopwatch = new Stopwatch();
			ElapsedTimeInfo = new ElapsedTimeInfo();
		} // constructor

		public ElapsedTimeInfo ElapsedTimeInfo { get; private set; } // ElapsedTimeInfo

		public void Execute(ElapsedDataMemberType nActionName, Action a) {
			if (a == null)
				return;

			m_oStopwatch.Restart();

			a();

			m_oStopwatch.Stop();

			ElapsedTimeInfo.IncreateData(nActionName, m_oStopwatch.ElapsedMilliseconds / 1000);
		} // Execute

		private readonly Stopwatch m_oStopwatch;

	} // class Stopper
} // namespace Ezbob.Utils
