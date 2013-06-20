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
	public abstract class AConnection : SafeLog, IConnection, IDisposable {
		#region public

		#region IDisposable implementation

		#region method Dispose

		public void Dispose() {
			// nothing to do here
		} // Dispose

		#endregion method Dispose

		#endregion IDisposable implementation

		#region IConnection implementation

		#region method ExecuteScalar

		public T ExecuteScalar<T>(string sQuery, params QueryParameter[] aryParams) {
			return ExecuteScalar<T>(sQuery, CommandSpecies.Auto, aryParams);
		} // ExecuteScalar

		public T ExecuteScalar<T>(string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams) {
			object oRes = Run(ExecMode.Scalar, nSpecies, sQuery, aryParams);

			if (oRes is DBNull)
				return default(T);

			return (T)oRes;
		} // ExecuteScalar

		#endregion method ExecuteScalar

		#region method ExecuteReader

		public DataTable ExecuteReader(string sQuery, params QueryParameter[] aryParams) {
			return ExecuteReader(sQuery, CommandSpecies.Auto, aryParams);
		} // ExecuteReader

		public DataTable ExecuteReader(string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams) {
			return (DataTable)Run(ExecMode.Reader, nSpecies, sQuery, aryParams);
		} // ExecuteReader

		#endregion method ExecuteReader

		#region method ExecuteNonQuery

		public int ExecuteNonQuery(string sQuery, params QueryParameter[] aryParams) {
			return ExecuteNonQuery(sQuery, CommandSpecies.Auto, aryParams);
		} // ExecuteNonQuery

		public int ExecuteNonQuery(string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams) {
			return (int)Run(ExecMode.NonQuery, nSpecies, sQuery, aryParams);
		} // ExecuteNonQuery

		#endregion method ExecuteNonQuery

		#endregion IConnection implementation

		#endregion public

		#region protected

		#region constructor

		protected AConnection(ASafeLog log = null, string sConnectionString = null) : base(log) {
			m_sConnectionString = sConnectionString;
		} // constructor

		#endregion constructor

		#region abstract methods

		protected abstract DbConnection CreateConnection();
		protected abstract DbCommand CreateCommand(string sCommand, DbConnection oConnection);
		protected abstract DbParameter CreateParameter(QueryParameter prm);

		#endregion abstract methods

		#region property ConnectionString

		protected virtual string ConnectionString {
			get {
				if (m_sConnectionString != null)
					return m_sConnectionString;

				var env = new Ezbob.Context.Environment();

				try {
					m_sConnectionString = ConfigurationManager.ConnectionStrings[env.Context.ToLower()].ConnectionString;
				}
				catch (Exception e) {
					string sMsg = string.Format(
						"Failed to load connection string from configuration file using name {0}",
						env.Context.ToLower()
					);

					Error(sMsg + ": " + e.Message);

					throw new Ezbob.Database.DbException(sMsg, e);
				} // try

				Info(string.Format("ConnectionString: {0}", m_sConnectionString));

				return m_sConnectionString;
			} // get
			set {
				m_sConnectionString = value;
			} // set
		} // ConnectionString

		private string m_sConnectionString;

		#endregion property ConnectionString

		#region property RetryCount

		protected virtual int RetryCount { get { return 3; } } // RetryCount

		#endregion property RetryCount

		#region method Retry

		protected virtual T Retry<T>(Func<T> func) {
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

		#endregion method Retry

		#region enum ExecMode

		protected enum ExecMode {
			Scalar,
			Reader,
			NonQuery
		} // Enum ExecMode

		#endregion enum ExecMode

		#region method Run

		protected virtual object Run(ExecMode nMode, CommandSpecies nSpecies, string spName, params QueryParameter[] aryParams) {
			var sb = new StringBuilder();

			foreach (var prm in aryParams)
				sb.Append(sb.Length > 0 ? ", " : " with parameters: ").Append(prm);

			Guid guid = Guid.NewGuid();
			Debug("Starting to run query:\n\tid = {0}\n\t{1}{2}", guid, spName, sb);

			try {
				return Retry(() => {
					using (var connection = CreateConnection()) {
						connection.Open();

						using (var command = CreateCommand(spName, connection)) {
							switch (nSpecies) {
							case CommandSpecies.Auto:
								command.CommandType = aryParams.Length == 0 ? CommandType.Text : CommandType.StoredProcedure;
								break;

							case CommandSpecies.StoredProcedure:
								command.CommandType = CommandType.StoredProcedure;
								break;

							case CommandSpecies.Text:
								command.CommandType = CommandType.Text;
								break;

							case CommandSpecies.TableDirect:
								command.CommandType = CommandType.TableDirect;
								break;

							default:
								throw new ArgumentOutOfRangeException("nSpecies");
							} // switch

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
								PublishRunningTime(guid, sw, string.Format("- {0} row{1} affected", nResult, nResult == 1 ? "" : "s"));
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

		#endregion method Retry

		#region method PublishRunningTime

		protected void PublishRunningTime(Guid guid, Stopwatch sw, string sMsg = "completed", string sAuxMsg = "") {
			sAuxMsg = (sAuxMsg ?? "").Trim();
			if (sAuxMsg != string.Empty)
				sAuxMsg = " " + sAuxMsg;

			Debug("Query {1} {2} in {0}ms{3}", sw.ElapsedMilliseconds, guid, sMsg, sAuxMsg);
		} // PublishRunnigTime

		#endregion method PublishRunningTime

		#endregion protected
	} // AConnection
} // namespace Ezbob.Database

