using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Ezbob.Logger;

namespace Ezbob.Database {
	#region class SqlConnection

	public class SqlConnection : AConnection {
		#region public

		#region constructor

		public SqlConnection(ASafeLog log = null) : base(log) {
		} // constructor

		public override DbConnection CreateConnection(string sConnectionString) {
			return new System.Data.SqlClient.SqlConnection(sConnectionString);
		} // CreateConnection

		public override DbCommand CreateCommand(string sCommand, DbConnection oConnection) {
			return new SqlCommand(sCommand, (System.Data.SqlClient.SqlConnection)oConnection);
		} // CreateCommand

		public override DbParameter CreateParameter(QueryParameter prm) {
			var oParam = (SqlParameter)CreateParameter(prm.Name, prm.Value);

			if (prm.Size != null)
				oParam.Size = (int)prm.Size;

			if (prm.Type != null)
				oParam.SqlDbType = (SqlDbType)prm.Type;

			return oParam;
		} // CreateParameter

		public virtual DbParameter CreateParameter(string sName, object oValue) {
			return (oValue is int && (int)oValue == 0)
					? new SqlParameter(sName, Convert.ToInt32(0))
					: new SqlParameter(sName, oValue);
		} // CreateParameter

		#endregion constructor

		#endregion public
	} // class SqlConnection

	#endregion class SqlConnection
} // namespace Ezbob.Database
