namespace Ezbob.Database {
	using System;
	using System.Data.Common;
	using System.Text;
	using System.Data;
	using System.Diagnostics;
	using System.Configuration;
	using System.Globalization;

	using Ezbob.Logger;
	using Utils;

	public abstract class AConnection : SafeLog, IConnection {
		#region public

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

		#region method ForEachRow

		public void ForEachRow(Func<DbDataReader, bool, ActionResult> oAction, string sQuery, params QueryParameter[] aryParams) {
			if (ReferenceEquals(oAction, null))
				throw new DbException("Callback action not specified in 'ForEachRow' call.");

			ForEachRow(oAction, sQuery, CommandSpecies.Auto, aryParams);
		} // ForEachRow

		public void ForEachRow(Func<DbDataReader, bool, ActionResult> oAction, string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams) {
			if (ReferenceEquals(oAction, null))
				throw new DbException("Callback action not specified in 'ForEachRow' call.");

			Run(oAction, ExecMode.ForEachRow, nSpecies, sQuery, aryParams);
		} // ForEachRow

		#endregion method ForEachRow

		#region method ForEachRowSafe

		public void ForEachRowSafe(Func<SafeReader, bool, ActionResult> oAction, string sQuery, params QueryParameter[] aryParams) {
			if (ReferenceEquals(oAction, null))
				throw new DbException("Callback action not specified in 'ForEachRow' call.");

			ForEachRowSafe(oAction, sQuery, CommandSpecies.Auto, aryParams);
		} // ForEachRowSafe

		public void ForEachRowSafe(Func<SafeReader, bool, ActionResult> oAction, string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams) {
			if (ReferenceEquals(oAction, null))
				throw new DbException("Callback action not specified in 'ForEachRow' call.");

			Run(	
				(oReader, bRowSetStart) => oAction(new SafeReader(oReader), bRowSetStart),
				ExecMode.ForEachRow, nSpecies, sQuery, aryParams
			);
		} // ForEachRowSafe

		#endregion method ForEachRowSafe

		public abstract string DateToString(DateTime oDate);

		#endregion IConnection implementation

		#region property LogVerbosityLevel

		public virtual LogVerbosityLevel LogVerbosityLevel {
			get { return m_nLogVerbosityLevel; }
			set { m_nLogVerbosityLevel = value; }
		} // LogVerbosityLevel

		private LogVerbosityLevel m_nLogVerbosityLevel;

		#endregion property LogVerbosityLevel

		#endregion public

		#region protected

		#region constructor

		protected AConnection(ASafeLog log = null, string sConnectionString = null) : base(log) {
			Env = new Ezbob.Context.Environment(log);
			m_sConnectionString = sConnectionString;
			m_nLogVerbosityLevel = LogVerbosityLevel.Compact;
		} // constructor

		protected AConnection(Ezbob.Context.Environment oEnv, ASafeLog log = null) : base(log) {
			Env = oEnv;
			m_sConnectionString = null;
			m_nLogVerbosityLevel = LogVerbosityLevel.Compact;
		} // Env

		#endregion constructor

		#region abstract methods

		protected abstract DbConnection CreateConnection();
		protected abstract DbCommand CreateCommand(string sCommand, DbConnection oConnection);
		protected abstract DbParameter CreateParameter(QueryParameter prm);

		protected abstract ARetryer CreateRetryer();

		#endregion abstract methods

		#region property Env

		protected virtual Ezbob.Context.Environment Env { get; private set; }

		#endregion property Env

		#region property ConnectionString

		protected virtual string ConnectionString {
			get {
				if (m_sConnectionString != null)
					return m_sConnectionString;

				try {
					m_sConnectionString = ConfigurationManager.ConnectionStrings[Env.Context.ToLower()].ConnectionString;
				}
				catch (Exception e) {
					string sMsg = string.Format(
						"Failed to load connection string from configuration file using name {0}",
						Env.Context.ToLower()
					);

					Error(sMsg + ": " + e.Message);

					throw new Ezbob.Database.DbException(sMsg, e);
				} // try

				Info("ConnectionString: {0}", m_sConnectionString);

				return m_sConnectionString;
			} // get
			set {
				m_sConnectionString = value;
			} // set
		} // ConnectionString

		private string m_sConnectionString;

		#endregion property ConnectionString

		#region enum ExecMode

		protected enum ExecMode {
			Scalar,
			Reader,
			NonQuery,
			ForEachRow
		} // Enum ExecMode

		#endregion enum ExecMode

		#region method Run

		protected virtual object Run(ExecMode nMode, CommandSpecies nSpecies, string spName, params QueryParameter[] aryParams) {
			return Run(null, nMode, nSpecies, spName, aryParams);
		} // Run

		protected virtual object Run(Func<DbDataReader, bool, ActionResult> oAction, ExecMode nMode, CommandSpecies nSpecies, string spName, params QueryParameter[] aryParams) {
			if ((nMode == ExecMode.ForEachRow) && ReferenceEquals(oAction, null))
				throw new DbException("Callback action not specified in 'ForEachRow' call.");

			var oArgsForLog = new StringBuilder();

			LogVerbosityLevel nLogVerbosityLevel = LogVerbosityLevel;

			foreach (var prm in aryParams)
				oArgsForLog.Append(oArgsForLog.Length > 0 ? ", " : string.Empty).Append(prm);

			string sArgsForLog = "(" + oArgsForLog + ")";

			Guid guid = Guid.NewGuid();

			if (nLogVerbosityLevel == LogVerbosityLevel.Verbose)
				Debug("Starting to run query:\n\tid = {0}\n\t{1}{2}", guid, spName, sArgsForLog);

			ARetryer oRetryer = CreateRetryer();
			oRetryer.LogVerbosityLevel = this.LogVerbosityLevel;

			try {
				return oRetryer.Retry<object>(() => {
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
								PublishRunningTime(nLogVerbosityLevel, spName, sArgsForLog, guid, sw);
								return value;

							case ExecMode.Reader: {
								var oReader = command.ExecuteReader();

								long nPrevStopwatchValue = sw.ElapsedMilliseconds;

								if (nLogVerbosityLevel == LogVerbosityLevel.Verbose)
									PublishRunningTime(nLogVerbosityLevel, spName, sArgsForLog, guid, sw);

								var dataTable = new DataTable();
								dataTable.Load(oReader);

								string sMsg = string.Empty;

								switch (nLogVerbosityLevel) {
								case LogVerbosityLevel.Compact:
									sMsg = "completed and data loaded";
									break;

								case LogVerbosityLevel.Verbose:
									sMsg = "data loaded";
									break;

								default:
									throw new ArgumentOutOfRangeException();
								} // switch

								PublishRunningTime(nLogVerbosityLevel, spName, sArgsForLog, guid, sw, nPrevStopwatchValue, sMsg);
								return dataTable;
							} // ExecMode.Reader

							case ExecMode.NonQuery: {
								int nResult = command.ExecuteNonQuery();
								string sResult = ((nResult == 0) || (nResult == -1)) ? "no" : nResult.ToString(CultureInfo.InvariantCulture);
								PublishRunningTime(nLogVerbosityLevel, spName, sArgsForLog, guid, sw, sAuxMsg: string.Format("- {0} row{1} changed", sResult, nResult == 1 ? "" : "s"));
								return nResult;
							} // ExecMode.NonQuery

							case ExecMode.ForEachRow: {
								command.ForEachRow(
									oAction,
									() => PublishRunningTime(nLogVerbosityLevel, spName, sArgsForLog, guid, sw)
								);

								return null;
							} // ExecMode.ForEachRow

							default:
								throw new ArgumentOutOfRangeException("nMode");
							} // switch
						} // using command
					} // using connection
				}); // Retry
			}
			catch (Exception e) {
				if (nLogVerbosityLevel == LogVerbosityLevel.Verbose)
					Error(e, "Error while executing query {0}", guid);
				else
					Error(e, "Error while executing query:\n\tid = {0}\n\t{1}{2}", guid, spName, sArgsForLog);

				throw;
			} // try
		} // Run

		#endregion method Run

		#region method PublishRunningTime

		protected void PublishRunningTime(
			LogVerbosityLevel nLogVerbosityLevel,
			string sSpName,
			string sArgsForLog,
			Guid guid,
			Stopwatch sw,
			long nPrevStopwatchValue = -1,
			string sMsg = "completed",
			string sAuxMsg = ""
		) {
			long nCurStopwatchValue = sw.ElapsedMilliseconds;

			sAuxMsg = (sAuxMsg ?? string.Empty).Trim();
			if (sAuxMsg != string.Empty)
				sAuxMsg = " " + sAuxMsg;

			switch (nLogVerbosityLevel) {
			case LogVerbosityLevel.Compact:
				string sTime;

				if (nPrevStopwatchValue == -1)
					sTime = nCurStopwatchValue + "ms";
				else
					sTime = nPrevStopwatchValue + "ms / " + (nCurStopwatchValue - nPrevStopwatchValue) + "ms";

				Debug("Query {0} in {1}{2}. Request: {3}{4}", sMsg, sTime, sAuxMsg, sSpName, sArgsForLog);
				break;

			case LogVerbosityLevel.Verbose:
				Debug("Query {1} {2} in {0}ms{3} since query start.", nCurStopwatchValue, guid, sMsg, sAuxMsg);
				break;

			default:
				throw new ArgumentOutOfRangeException("nLogVerbosityLevel");
			} // switch
		} // PublishRunnigTime

		#endregion method PublishRunningTime

		#endregion protected
	} // AConnection
} // namespace Ezbob.Database
