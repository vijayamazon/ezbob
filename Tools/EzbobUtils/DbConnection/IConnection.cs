namespace Ezbob.Database {
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.ComponentModel;
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
		[Description("raw text/stored procedure")]
		Auto,

		[Description("stored procedure")]
		StoredProcedure,

		[Description("raw text")]
		Text,

		[Description("table direct")]
		TableDirect
	} // enum CommandSpecies

	#endregion enum CommandSpecies

	#region interface IParameterisable

	public interface IParametrisable {
		object[] ToParameter();
	} // interface IParametrisable

	#endregion interface IParameterisable

	#region interface IConnection

	public interface IConnection {
		T ExecuteScalar<T>(string sQuery, params QueryParameter[] aryParams);
		DataTable ExecuteReader(string sQuery, params QueryParameter[] aryParams);
		int ExecuteNonQuery(string sQuery, params QueryParameter[] aryParams);
		void ForEachRow(Func<DbDataReader, bool, ActionResult> oAction, string sQuery, params QueryParameter[] aryParams);
		void ForEachRowSafe(Func<SafeReader, bool, ActionResult> oAction, string sQuery, params QueryParameter[] aryParams);
		void ForEachResult<T>(Func<T, ActionResult> oAction, string sQuery, params QueryParameter[] aryParams) where T: IResultRow, new();
		List<T> Fill<T>(string sQuery, params QueryParameter[] aryParams) where T: ITraversable, new();
		T FillFirst<T>(string sQuery, params QueryParameter[] aryParams) where T: ITraversable, new();

		T ExecuteScalar<T>(string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams);
		DataTable ExecuteReader(string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams);
		int ExecuteNonQuery(string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams);
		void ForEachRow(Func<DbDataReader, bool, ActionResult> oAction, string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams);
		void ForEachRowSafe(Func<SafeReader, bool, ActionResult> oAction, string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams);
		void ForEachResult<T>(Func<T, ActionResult> oAction, string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams) where T: IResultRow, new();
		List<T> Fill<T>(string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams) where T: ITraversable, new();
		T FillFirst<T>(string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams) where T: ITraversable, new();

		string DateToString(DateTime oDate);

		QueryParameter CreateVectorParameter<T>(string sFieldName, IEnumerable<T> oValues);
		QueryParameter CreateVectorParameter<T>(string sFieldName, params T[] oValues);

		QueryParameter CreateTableParameter<TColumnInfo, TSource>(string sFieldName, IEnumerable<TSource> oValues, ParameterDirection nDirection = ParameterDirection.Input)
			where TColumnInfo : ITraversable, new()
			where TSource: IParametrisable;

		QueryParameter CreateTableParameter<TColumnInfo>(
			string sFieldName,
			IEnumerable oValues,
			Func<object, object[]> oValueToRow,
			ParameterDirection nDirection = ParameterDirection.Input
		) where TColumnInfo : ITraversable, new();

		QueryParameter CreateTableParameter(
			Type oColumnInfo,
			string sFieldName,
			IEnumerable oValues,
			Func<object, object[]> oValueToRow,
			ParameterDirection nDirection = ParameterDirection.Input
		);
	} // IConnection

	#endregion interface IConnection
} // namespace Ezbob.Database
