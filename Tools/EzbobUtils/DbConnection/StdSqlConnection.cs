namespace Ezbob.Database {
	using System.Data.Common;
	using Logger;

	#region class StdSqlConnection

	public class StdSqlConnection : SqlConnection {
		#region public

		#region constructor

		public StdSqlConnection(System.Data.SqlClient.SqlConnection oDB, ASafeLog log = null) : base(log) {
			m_oDB = oDB;
		} // constructor

		#endregion constructor

		#endregion public

		#region protected

		#region method CreateConnection

		protected override DbConnection CreateConnection() {
			return m_oDB;
		} // CreateConnection

		#endregion method CreateConnection

		#endregion protected

		#region private

		private readonly System.Data.SqlClient.SqlConnection m_oDB;

		#endregion private
	} // class StdSqlConnection

	#endregion class StdSqlConnection
} // namespace Ezbob.Database
