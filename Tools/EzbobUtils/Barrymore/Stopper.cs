namespace Ezbob.Utils {
	using System;
	using System.Diagnostics;

	public class Stopper {
		#region public

		#region constructor

		public Stopper() {
			m_oStopwatch = new Stopwatch();
			ElapsedTimeInfo = new ElapsedTimeInfo();
		} // constructor

		#endregion constructor

		#region property ElapsedTimeInfo

		public ElapsedTimeInfo ElapsedTimeInfo { get; private set; } // ElapsedTimeInfo

		#endregion property ElapsedTimeInfo

		#region method Execute

		public void Execute(ElapsedDataMemberType nActionName, Action a) {
			if (a == null)
				return;

			m_oStopwatch.Restart();

			a();

			m_oStopwatch.Stop();

			ElapsedTimeInfo.IncreateData(nActionName, m_oStopwatch.ElapsedMilliseconds / 1000);
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private readonly Stopwatch m_oStopwatch;

		#endregion private
	} // class Stopper
} // namespace Ezbob.Utils
