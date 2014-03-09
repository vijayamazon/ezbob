﻿namespace Ezbob.Database {
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

		#endregion public

		#region protected

		#region method CreateConnection

		protected override DbConnection CreateConnection() {
			return new System.Data.SqlClient.SqlConnection(ConnectionString);
		} // CreateConnection

		#endregion method CreateConnection

		#region method CreateCommand

		protected override DbCommand CreateCommand(string sCommand, DbConnection oConnection) {
			return new SqlCommand(sCommand, (System.Data.SqlClient.SqlConnection)oConnection);
		} // CreateCommand

		#endregion method CreateCommand

		#region method CreateParameter

		protected override DbParameter CreateParameter(QueryParameter prm) {
			var oParam = (SqlParameter)CreateParameter(prm.Name, prm.Value);

			if (prm.Size != null)
				oParam.Size = (int)prm.Size;

			if (prm.Type != null)
				oParam.SqlDbType = (SqlDbType)prm.Type;

			if (!string.IsNullOrWhiteSpace(prm.UnderlyingTypeName))
				oParam.UdtTypeName = prm.UnderlyingTypeName;

			oParam.Direction = prm.Direction;

			prm.UnderlyingParameter = oParam;

			return oParam;
		} // CreateParameter

		protected virtual DbParameter CreateParameter(string sName, object oValue) {
			return (oValue is int && (int)oValue == 0)
					? new SqlParameter(sName, Convert.ToInt32(0))
					: new SqlParameter(sName, oValue);
		} // CreateParameter

		#endregion method CreateParameter

		#region method CreateRetryer

		protected override Utils.ARetryer CreateRetryer() {
			return new SqlRetryer(nRetryCount: 3, nSleepBeforeRetryMilliseconds: 500, oLog: this);
		} // CreateRetryer

		#endregion method CreateRetryer

		#endregion protected
	} // class SqlConnection

	#endregion class SqlConnection
} // namespace Ezbob.Database
