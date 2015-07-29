namespace FraudChecker {
	using System;
	using System.Collections.Generic;
	using EZBob.DatabaseLib.Model;

	internal class SessionInfo {
		public SessionInfo(CustomerSession cs) {
			ID = cs.Id;
			Ip = cs.Ip;
			StartSessionTime = cs.StartSession;
		} // constructor

		public bool IsSuspicious(CustomerSession cs) {
			if (cs == null)
				return false;

			return 
				(Ip == cs.Ip) &&
				(Math.Abs((StartSessionTime - cs.StartSession).TotalDays) <= 30);
		} // IsSuspicious

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString() {
			return string.Format("(id {2}) {0} at {1}", Ip, StartSessionTime.ToString("d/MMM/yyyy HH:mm:ss"), ID);
		} // ToString

		public int ID { get; private set; }
		public string Ip { get; private set; }
		public DateTime StartSessionTime { get; private set; }
	} // class SessionInfo

	internal class SessionInfoComparer : IEqualityComparer<SessionInfo> {
		public bool Equals(SessionInfo x, SessionInfo y) {
			if ((x == null) && (y == null))
				return true;

			if ((x == null) || (y == null))
				return false;

			return (x.Ip == y.Ip) && (x.StartSessionTime == y.StartSessionTime);
		} // Equals

		public int GetHashCode(SessionInfo obj) {
			if (obj == null)
				throw new ArgumentNullException("obj", "GetHashCode((SessionInfo)null)");

			return (obj.Ip + "|" + obj.StartSessionTime).GetHashCode();
		} // GetHashCode
	} // class SessionInfoComparer
} // namespace
