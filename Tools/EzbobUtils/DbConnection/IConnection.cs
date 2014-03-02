namespace Ezbob.Database {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Data.Common;
	using Ezbob.Utils;

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

	#region interface IConnection

	public interface IConnection {
		T ExecuteScalar<T>(string sQuery, params QueryParameter[] aryParams);
		DataTable ExecuteReader(string sQuery, params QueryParameter[] aryParams);
		int ExecuteNonQuery(string sQuery, params QueryParameter[] aryParams);
		void ForEachRow(Func<DbDataReader, bool, ActionResult> oAction, string sQuery, params QueryParameter[] aryParams);
		void ForEachRowSafe(Func<SafeReader, bool, ActionResult> oAction, string sQuery, params QueryParameter[] aryParams);
		List<T> Fill<T>(string sQuery, params QueryParameter[] aryParams) where T: ITraversable, new();
		T FillFirst<T>(string sQuery, params QueryParameter[] aryParams) where T: ITraversable, new();

		T ExecuteScalar<T>(string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams);
		DataTable ExecuteReader(string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams);
		int ExecuteNonQuery(string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams);
		void ForEachRow(Func<DbDataReader, bool, ActionResult> oAction, string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams);
		void ForEachRowSafe(Func<SafeReader, bool, ActionResult> oAction, string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams);
		List<T> Fill<T>(string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams) where T: ITraversable, new();
		T FillFirst<T>(string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams) where T: ITraversable, new();

		string DateToString(DateTime oDate);
	} // IConnection

	#endregion interface IConnection
} // namespace Ezbob.Database
