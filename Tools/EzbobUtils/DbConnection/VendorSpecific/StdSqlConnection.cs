namespace Ezbob.Database {
	using System.Data.Common;
	using Logger;

	public class StdSqlConnection : SqlConnection {

		public StdSqlConnection(System.Data.SqlClient.SqlConnection oDB, ASafeLog log = null) : base(log) {
			m_oDB = oDB;
		} // constructor

		protected override DbConnection CreateConnection() {
			return m_oDB;
		} // CreateConnection

		private readonly System.Data.SqlClient.SqlConnection m_oDB;

	} // class StdSqlConnection

} // namespace Ezbob.Database
