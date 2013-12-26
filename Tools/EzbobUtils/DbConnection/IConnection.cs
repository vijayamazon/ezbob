using System;
using System.Data;
using System.Data.Common;

namespace Ezbob.Database {
	#region enum ActionResult

	public enum ActionResult {
		Continue,
		SkipCurrent,
		SkipAll
	} // ActionResult

	#endregion enum ActionResult

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

	#region enum LogVerbosityLevel

	public enum LogVerbosityLevel {
		Compact,
		Verbose
	} // enum LogVerbosityLevel

	#endregion enum LogVerbosityLevel

	#region interface IConnection

	public interface IConnection {
		T ExecuteScalar<T>(string sQuery, params QueryParameter[] aryParams);
		DataTable ExecuteReader(string sQuery, params QueryParameter[] aryParams);
		int ExecuteNonQuery(string sQuery, params QueryParameter[] aryParams);
		void ForEachRow(Func<DbDataReader, bool, ActionResult> oAction, string sQuery, params QueryParameter[] aryParams);

		T ExecuteScalar<T>(string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams);
		DataTable ExecuteReader(string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams);
		int ExecuteNonQuery(string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams);
		void ForEachRow(Func<DbDataReader, bool, ActionResult> oAction, string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams);

		string DateToString(DateTime oDate);
	} // IConnection

	#endregion interface IConnection
} // namespace Ezbob.Database
