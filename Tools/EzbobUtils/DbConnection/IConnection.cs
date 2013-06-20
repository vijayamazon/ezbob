using System.Data;

namespace Ezbob.Database {
	public interface IConnection {
		T ExecuteScalar<T>(string sQuery, params QueryParameter[] aryParams);
		DataTable ExecuteReader(string sQuery, params QueryParameter[] aryParams);
		int ExecuteNonQuery(string sQuery, params QueryParameter[] aryParams);
	} // IConnection
} // namespace Ezbob.Database
