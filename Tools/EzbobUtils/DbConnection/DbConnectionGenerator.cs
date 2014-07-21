namespace Ezbob.Database {
	using Logger;

	public static class DbConnectionGenerator {
		static DbConnectionGenerator() {
			ms_oLock = new object();
		} // static constructor

		public static AConnection Get(ASafeLog oLog = null) {
			if (!ReferenceEquals(null, ms_oDB))
				return ms_oDB;

			if (oLog == null)
				oLog = new SafeILog(typeof(DbConnectionGenerator));

			var env = new Context.Environment(oLog);

			lock (ms_oLock) {
				if (ReferenceEquals(null, ms_oDB))
					ms_oDB = new SqlConnection(env, oLog);
			} // lock

			return ms_oDB;
		} // operator AConnection

		private static AConnection ms_oDB;
		private static readonly object ms_oLock;
	} // class DbConnectionGenerator
} // namespace