namespace EzBob.Web.Code {
	using Ezbob.Database;
	using Ezbob.Logger;
	using log4net;

	public static class DbConnectionGenerator {
		static DbConnectionGenerator() {
			ms_oLock = new object();
		} // static constructor

		public static AConnection Get() {
			if (!ReferenceEquals(null, ms_oDB))
				return ms_oDB;

			lock (ms_oLock) {
				var oLog = new SafeILog(LogManager.GetLogger(typeof (DbConnectionGenerator)));
				var env = new Ezbob.Context.Environment(oLog);
				ms_oDB = new Ezbob.Database.SqlConnection(env, oLog);
			} // lock

			return ms_oDB;
		} // operator AConnection

		private static AConnection ms_oDB;
		private static readonly object ms_oLock;
	} // class DbConnectionGenerator
} // namespace