namespace Ezbob.Database {
	using System;
	using System.Data;
	using System.Data.Common;
	using System.Data.SqlClient;
	using System.Globalization;
	using Ezbob.Logger;

	public class SqlConnection : AConnection {
		public SqlConnection(ASafeLog log = null, string sConnectionString = null) : base(log, sConnectionString) {
		} // constructor

		public SqlConnection(Ezbob.Context.Environment oEnv, ASafeLog log = null) : base(oEnv, log) {
		} // constructor

		public override string DateToString(DateTime oDate) {
			return oDate.ToString("MMMM d yyyy H:mm:ss", CultureInfo.InvariantCulture);
		} // DateToString

		public override QueryParameter BuildTableParameter(string sFieldName, DataTable oValues) {
			return new QueryParameter(new SqlParameter(sFieldName, SqlDbType.Structured) {
				Value = oValues,
				Direction = ParameterDirection.Input,
			});
		} // CreateTableParameter

		protected override DbConnection CreateConnection() {
			var oConnection = new System.Data.SqlClient.SqlConnection(ConnectionString);

			oConnection.InfoMessage += (sender, args) => Log.Debug(
				"Database information from '{0}'; message: '{1}'.\nErrors:\n\t{2}",
				args.Source,
				args.Message,
				string.Join("\n\t", args.Errors)
			);

			//oConnection.StateChange += (sender, args) => Log.Debug(
			//	"Database connection state change: {0} -> {1}.",
			//	args.OriginalState,
			//	args.CurrentState
			//);

			oConnection.Disposed += (obj, args) => Log.Debug("Database connection is disposed.");

			return oConnection;
		} // CreateConnection

		protected override DbCommand CreateCommand(string sCommand) {
			return new SqlCommand { CommandText = sCommand, };
		} // CreateCommand

		protected override void AppendParameter(DbCommand cmd, QueryParameter prm) {
			var oCmd = cmd as SqlCommand;

			if (oCmd == null) {
				throw new DbException(
					"Something went terribly wrong: " +
					"SQL Server command argument contains non-SQL Server underlying parameter."
				);
			} // if

			if (!ReferenceEquals(prm.UnderlyingParameter, null) && (prm.UnderlyingParameter is SqlParameter)) {
				oCmd.Parameters.Add(prm.UnderlyingParameter);
				return;
			} // if

			SqlParameter oParam;

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

				if (prm.Value is int && (int)prm.Value == 0) {
					// ReSharper disable RedundantCast
					oValue = (int)0;
					// ReSharper restore RedundantCast
				}
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

		protected override SqlRetryer CreateRetryer() {
			var rnd = new Random();

			int nSleepBeforeRetryMilliseconds = 400 + rnd.Next(200);

			return new SqlRetryer(3, nSleepBeforeRetryMilliseconds, Log);
		} // CreateRetryer
	} // class SqlConnection
} // namespace Ezbob.Database
