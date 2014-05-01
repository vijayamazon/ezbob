namespace Ezbob.Database
{
	using Logger;
	using log4net;

	public static class DbConnectionGenerator {
		static DbConnectionGenerator() {
			ms_oLock = new object();
		} // static constructor

		public static AConnection Get(ASafeLog oLog = null) {
			if (!ReferenceEquals(null, ms_oDB))
				return ms_oDB;

			if (oLog == null)
				oLog = new SafeILog(LogManager.GetLogger(typeof (DbConnectionGenerator)));

			var env = new Context.Environment(oLog);

			lock (ms_oLock) {
				ms_oDB = new SqlConnection(env, oLog);
			} // lock

			return ms_oDB;
		} // operator AConnection

		private static AConnection ms_oDB;
		private static readonly object ms_oLock;
	} // class DbConnectionGenerator
} // namespace