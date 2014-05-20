namespace Ezbob.Database {
	using System;
	using System.Data;
	using System.Data.Common;
	using System.Data.SqlClient;
	using System.Globalization;
	using Ezbob.Logger;

	#region class SqlConnection

	public class SqlConnection : AConnection {
		#region public

		#region constructor

		public SqlConnection(ASafeLog log = null, string sConnectionString = null) : base(log, sConnectionString) {
		} // constructor

		public SqlConnection(Ezbob.Context.Environment oEnv, ASafeLog log = null) : base(oEnv, log) {
		} // constructor

		#endregion constructor

		#region method DateToString

		public override string DateToString(DateTime oDate) {
			return oDate.ToString("MMMM d yyyy H:mm:ss", CultureInfo.InvariantCulture);
		} // DateToString

		#endregion method DateToString

		#region method CreateTableParameter

		public override QueryParameter BuildTableParameter(string sFieldName, DataTable oValues, ParameterDirection nDirection = ParameterDirection.Input) {
			return new QueryParameter(new SqlParameter(sFieldName, SqlDbType.Structured) {
				Value = oValues,
				Direction = nDirection,
			});
		} // CreateTableParameter

		#endregion method CreateTableParameter

		public override DbCommand NewCommand {
			get {
				var oCmd = new SqlCommand();
				oCmd.Connection = new System.Data.SqlClient.SqlConnection(ConnectionString);
				return oCmd;
			} // get
		} // NewCommand

		#endregion public

		#region protected

		#region method CreateConnection

		protected override DbConnection CreateConnection() {
			return new System.Data.SqlClient.SqlConnection(ConnectionString);
		} // CreateConnection

		#endregion method CreateConnection

		#region method CreateCommand

		protected override DbCommand CreateCommand(string sCommand, DbConnection oConnection) {
			var oCmd = new SqlCommand();
			oCmd.CommandText = sCommand;

			if (oConnection != null)
				oCmd.Connection = (System.Data.SqlClient.SqlConnection)oConnection;

			return oCmd;
		} // CreateCommand

		#endregion method CreateCommand

		#region method AppendParameter

		protected override void AppendParameter(DbCommand cmd, QueryParameter prm) {
			var oCmd = cmd as SqlCommand;

			if (oCmd == null)
				throw new DbException("Something went terribly wrong: SQL Server command argument contains non-SQL Server underlying parameter.");

			if (!ReferenceEquals(prm.UnderlyingParameter, null) && (prm.UnderlyingParameter is SqlParameter)) {
				oCmd.Parameters.Add(prm.UnderlyingParameter);
				return;
			} // if

			SqlParameter oParam = null;

			if (prm.Type == DbType.Binary) {
				oParam = oCmd.Parameters.Add(prm.Name, SqlDbType.VarBinary);

				byte[] oValue = (prm.Value == DBNull.Value) ? null : (prm.Value as byte[]);

				if ((oValue == null) || (oValue.Length < 1)) {
					oParam.Size = 0;
					oParam.Value = DBNull.Value;
				}
				else {
					oParam.Size = oValue.Length;
					oParam.Value = oValue;
				} // if

				oParam.Precision = 0;
				oParam.Scale = 0;
			}
			else {
				object oValue;

				if (prm.Value is int && (int)prm.Value == 0)
					oValue = (int)0;
				else
					oValue = prm.Value;

				oParam = oCmd.Parameters.AddWithValue(prm.Name, oValue);

				if (prm.Type != null)
					oParam.DbType = prm.Type.Value;

				if (prm.Size != null)
					oParam.Size = prm.Size.Value;
			} // if

			if (!string.IsNullOrWhiteSpace(prm.UnderlyingTypeName))
				oParam.TypeName = prm.UnderlyingTypeName;

			oParam.Direction = prm.Direction;

			prm.UnderlyingParameter = oParam;
		} // AppendParameter

		#endregion method AppendParameter

		#region method CreateRetryer

		protected override Utils.ARetryer CreateRetryer() {
			return new SqlRetryer(nRetryCount: 3, nSleepBeforeRetryMilliseconds: 500, oLog: this);
		} // CreateRetryer

		#endregion method CreateRetryer

		#endregion protected
	} // class SqlConnection

	#endregion class SqlConnection
} // namespace Ezbob.Database
