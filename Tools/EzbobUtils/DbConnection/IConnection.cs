using System.Data;

namespace Ezbob.Database {
	#region enum CommandSpecies

	public enum CommandSpecies {
		/// <summary>
		/// With parameters: stored proc.
		/// Without parameters: text.
		/// </summary>
		Auto,
		StoredProcedure,
		Text,
		TableDirect
	} // enum CommandSpecies

	#endregion enum CommandSpecies

	public interface IConnection {
		T ExecuteScalar<T>(string sQuery, params QueryParameter[] aryParams);
		DataTable ExecuteReader(string sQuery, params QueryParameter[] aryParams);
		int ExecuteNonQuery(string sQuery, params QueryParameter[] aryParams);

		T ExecuteScalar<T>(string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams);
		DataTable ExecuteReader(string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams);
		int ExecuteNonQuery(string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams);
	} // IConnection
} // namespace Ezbob.Database
