using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading;
using System.Text;
using System.Data;
using System.Diagnostics;
using System.Configuration;

using Ezbob.Logger;

namespace Ezbob.Database {
	public abstract class AConnection : SafeLog, IConnection, IDisposable  {
		private const int RetryCount = 3;

		private string m_sConnectionString;

		private enum ExecMode {
			Scalar,
			Reader,
			NonQuery
		} // Enum ExecMode

		public AConnection(ASafeLog log = null) : base(log) {
			var env = new Ezbob.Context.Environment();

			try {
				m_sConnectionString = ConfigurationManager.ConnectionStrings[env.Context.ToLower()].ConnectionString;
			}
			catch (Exception) {
				Error("Failed to load connection string from configuration file using name {0}", env.Context.ToLower());
				throw;
			}

			Info(string.Format("ConnectionString: {0}", m_sConnectionString));
		} // constructor

		public abstract DbConnection CreateConnection(string sConnectionString);
		public abstract DbCommand CreateCommand(string sCommand, DbConnection oConnection);
		public abstract DbParameter CreateParameter(QueryParameter prm);

		private T Retry<T>(Func<T> func) {
			int nCount = RetryCount;

			while (true) {
				try {
					return func();
				}
				catch (SqlException e) {
					--nCount;
					if (nCount <= 0)
						throw;

					if (e.Number == 1205)
						Warn(string.Format("Deadlock, retrying {0}", e));
					else if (e.Number == -2)
						Warn(string.Format("Timeout, retrying {0}", e));
					else
						throw;

					Thread.Sleep(TimeSpan.FromSeconds(5));
				} // try
			} // while
		} // Retry

		public T ExecuteScalar<T>(string sQuery, params QueryParameter[] aryParams) {
			object oRes = Run(ExecMode.Scalar, sQuery, aryParams);

			if (oRes is DBNull)
				return default(T);

			return (T)oRes;
		} // ExecuteScalar

		public DataTable ExecuteReader(string sQuery, params QueryParameter[] aryParams) {
			return (DataTable)Run(ExecMode.Reader, sQuery, aryParams);
		} // ExecuteReader

		public int ExecuteNonQuery(string sQuery, params QueryParameter[] aryParams) {
			return (int)Run(ExecMode.NonQuery, sQuery, aryParams);
		} // ExecuteNonQuery

		private object Run(ExecMode nMode, string spName, params QueryParameter[] aryParams) {
			var sb = new StringBuilder();

			foreach (var prm in aryParams)
				sb.Append(sb.Length > 0 ? ", " : " with parameters: ").Append(prm);

			Guid guid = Guid.NewGuid();
			Debug("Starting to run query:\n\tid = {0}\n\t{1}{2}", guid, spName, sb);

			try {
				return Retry(() => {
					using (var connection = CreateConnection(m_sConnectionString)) {
						connection.Open();

						using (var command = CreateCommand(spName, connection)) {
							command.CommandType = aryParams.Length == 0 ? CommandType.Text : CommandType.StoredProcedure;
							foreach (var prm in aryParams)
								command.Parameters.Add(CreateParameter(prm));

							var sw = new Stopwatch();
							sw.Start();

							switch (nMode) {
							case ExecMode.Scalar:
								object value = command.ExecuteScalar();
								PublishRunningTime(guid, sw);
								return value;

							case ExecMode.Reader:
								var oReader = command.ExecuteReader();
								PublishRunningTime(guid, sw);
								var dataTable = new DataTable();
								dataTable.Load(oReader);
								PublishRunningTime(guid, sw, "data loaded");
								return dataTable;

							case ExecMode.NonQuery:
								int nResult = command.ExecuteNonQuery();
								PublishRunningTime(guid, sw);
								return nResult;

							default:
								throw new ArgumentOutOfRangeException("nMode");
							} // switch
						} // using command
					} // using connection
				}); // Retry
			}
			catch (Exception e) {
				Error("Error while executing query {0}:\n{1}", guid, e);
				throw;
			} // try
		} // Run

		private void PublishRunningTime(Guid guid, Stopwatch sw, string sMsg = "completed") {
			Debug("Query {1} {2} in {0}ms", sw.ElapsedMilliseconds, guid, sMsg);
		} // PublishRunnigTime

		public void Dispose() {
			// nothing to do here
		} // Dispose
	} // AConnection
} // namespace Ezbob.Database

