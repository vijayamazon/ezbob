namespace Ezbob.Database {
	using System;
	using System.Collections.Generic;
	using System.Data.Common;
	using System.Text;
	using System.Data;
	using System.Diagnostics;
	using System.Configuration;
	using System.Globalization;

	using Ezbob.Logger;
	using Pool;
	using Utils;

	public abstract partial class AConnection : IDisposable {
		public virtual void Dispose() {
			// nothing here so far.
		} // Dispose

		public virtual int CommandTimeout {
			get { return m_nCommandTimeout; }
			set { m_nCommandTimeout = value; }
		} // CommandTimeout

		public virtual Ezbob.Context.Environment Env { get; private set; }

		public virtual LogVerbosityLevel LogVerbosityLevel {
			get { return m_nLogVerbosityLevel; }
			set { m_nLogVerbosityLevel = value; }
		} // LogVerbosityLevel

		public class VectorToTableParameter<T> {
			public T Value { get; set; } // Value
		} // class VectorToTableParameter

		public static void UpdateConnectionPoolMaxSize(int nMaxSize) {
			ms_oPool.MaxSize = nMaxSize;
		} // UpdateConnectionPoolMaxSize

		public abstract QueryParameter BuildTableParameter(string sFieldName, DataTable oValues);

		public virtual QueryParameter CreateVectorParameter<T>(string sFieldName, params T[] oValues) {
			return CreateVectorParameter<T>(sFieldName, (IEnumerable<T>)oValues);
		} // CreateVectorParameter

		public virtual QueryParameter CreateVectorParameter<T>(string sFieldName, IEnumerable<T> oValues) {
			return CreateTableParameter<VectorToTableParameter<T>>(sFieldName, oValues, oOneValue => new object[] { oOneValue });
		} // CreateVectorParameter

		public abstract string DateToString(DateTime oDate);

		public virtual void DisposeAfterOneUsage(bool bAllesInOrdnung, ConnectionWrapper oConnection) {
			if (oConnection == null)
				return;

			if (bAllesInOrdnung)
				ms_oPool.Take(oConnection.Pooled);
			else
				ms_oPool.Drop(oConnection.Pooled);
		} // DisposeAfterOneUsage

		public virtual IEnumerable<SafeReader> ExecuteEnumerable(ConnectionWrapper oConnectionToUse, string sQuery, params QueryParameter[] aryParams) {
			return ExecuteEnumerable(oConnectionToUse, sQuery, CommandSpecies.Auto, aryParams);
		} // ExecuteEnumerable

		public virtual IEnumerable<SafeReader> ExecuteEnumerable(string sQuery, params QueryParameter[] aryParams) {
			return ExecuteEnumerable(null, sQuery, CommandSpecies.Auto, aryParams);
		} // ExecuteEnumerable

		public virtual IEnumerable<SafeReader> ExecuteEnumerable(string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams) {
			return ExecuteEnumerable(null, sQuery, nSpecies, aryParams);
		} // ExecuteEnumerable

		public virtual IEnumerable<SafeReader> ExecuteEnumerable(ConnectionWrapper oConnectionToUse, string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams) {
			return (IEnumerable<SafeReader>)Run(oConnectionToUse, null, ExecMode.Enumerable, nSpecies, sQuery, aryParams);
		} // ExecuteEnumerable

		public virtual int ExecuteNonQuery(ConnectionWrapper oConnectionToUse, string sQuery, params QueryParameter[] aryParams) {
			return ExecuteNonQuery(oConnectionToUse, sQuery, CommandSpecies.Auto, aryParams);
		} // ExecuteNonQuery

		public virtual int ExecuteNonQuery(string sQuery, params QueryParameter[] aryParams) {
			return ExecuteNonQuery(null, sQuery, CommandSpecies.Auto, aryParams);
		} // ExecuteNonQuery

		public virtual int ExecuteNonQuery(string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams) {
			return ExecuteNonQuery(null, sQuery, nSpecies, aryParams);
		} // ExecuteNonQuery

		public virtual int ExecuteNonQuery(ConnectionWrapper oConnectionToUse, string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams) {
			return (int)Run(oConnectionToUse, ExecMode.NonQuery, nSpecies, sQuery, aryParams);
		} // ExecuteNonQuery

		public virtual T ExecuteScalar<T>(ConnectionWrapper oConnectionToUse, string sQuery, params QueryParameter[] aryParams) {
			return ExecuteScalar<T>(oConnectionToUse, sQuery, CommandSpecies.Auto, aryParams);
		} // ExecuteScalar

		public virtual T ExecuteScalar<T>(string sQuery, params QueryParameter[] aryParams) {
			return ExecuteScalar<T>(null, sQuery, CommandSpecies.Auto, aryParams);
		} // ExecuteScalar

		public virtual T ExecuteScalar<T>(string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams) {
			return ExecuteScalar<T>(null, sQuery, nSpecies, aryParams);
		} // ExecuteScalar

		public virtual T ExecuteScalar<T>(ConnectionWrapper oConnectionToUse, string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams) {
			object oRes = Run(oConnectionToUse, ExecMode.Scalar, nSpecies, sQuery, aryParams);

			if ((oRes == null) || (oRes is DBNull))
				return default(T);

			return (T)oRes;
		} // ExecuteScalar

		public List<T> Fill<T>(ConnectionWrapper oConnectionToUse, string sQuery, params QueryParameter[] aryParams) where T : new() {
			return Fill<T>(oConnectionToUse, sQuery, CommandSpecies.Auto, aryParams);
		} // Fill

		public List<T> Fill<T>(string sQuery, params QueryParameter[] aryParams) where T : new() {
			return Fill<T>(null, sQuery, CommandSpecies.Auto, aryParams);
		} // Fill

		public List<T> Fill<T>(string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams) where T : new() {
			return Fill<T>(null, sQuery, nSpecies, aryParams);
		} // Fill

		public List<T> Fill<T>(ConnectionWrapper oConnectionToUse, string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams) where T : new() {
			var oResult = new List<T>();

			ForEachRowSafe(
				oConnectionToUse,
				(sr, bRowsetStart) => {
					oResult.Add(sr.Fill<T>());
					return ActionResult.Continue;
				},
				sQuery,
				nSpecies,
				aryParams
			);

			return oResult;
		} // Fill

		public virtual T FillFirst<T>(ConnectionWrapper oConnectionToUse, string sQuery, params QueryParameter[] aryParams) where T : new() {
			return FillFirst<T>(oConnectionToUse, sQuery, CommandSpecies.Auto, aryParams);
		} // FillFirst

		public virtual T FillFirst<T>(string sQuery, params QueryParameter[] aryParams) where T : new() {
			return FillFirst<T>(null, sQuery, CommandSpecies.Auto, aryParams);
		} // FillFirst

		public virtual T FillFirst<T>(string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams) where T : new() {
			return FillFirst<T>(null, sQuery, nSpecies, aryParams);
		} // FillFirst

		public virtual T FillFirst<T>(ConnectionWrapper oConnectionToUse, string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams) where T : new() {
			var oResult = new T();

			ForEachRowSafe(
				oConnectionToUse,
				(sr, bRowsetStart) => {
					sr.Fill(oResult);
					return ActionResult.SkipAll;
				},
				sQuery,
				nSpecies,
				aryParams
			);

			return oResult;
		} // FillFirst

		public virtual void FillFirst<T>(T oInstatnce, string sQuery, params QueryParameter[] aryParams) {
			FillFirst<T>(null, oInstatnce, sQuery, CommandSpecies.Auto, aryParams);
		} // FillFirst

		public virtual void FillFirst<T>(T oInstance, string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams) {
			FillFirst<T>(null, oInstance, sQuery, nSpecies, aryParams);
		} // FillFirst

		public virtual void FillFirst<T>(ConnectionWrapper oConnectionToUse, T oInstance, string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams) {
			if (!typeof(T).IsValueType) {
				// Plain comparison "oInstance == null" fires warning "possible compare of value type with null".
				// Assignment to temp variable is a workaround to suppress the warning.
				// And if we are already here then T is not a value type so it can be null.
				object obj = oInstance;

				if (obj == null)
					throw new NullReferenceException("Cannot FillFirst of type " + typeof(T) + ": no instance specified.");
			} // if

			ForEachRowSafe(
				oConnectionToUse,
				(sr, bRowsetStart) => {
					sr.Fill(oInstance);
					return ActionResult.SkipAll;
				},
				sQuery,
				nSpecies,
				aryParams
			);
		} // FillFirst

		public virtual SafeReader GetFirst(ConnectionWrapper oConnectionToUse, string sQuery, params QueryParameter[] aryParams) {
			return GetFirst(oConnectionToUse, sQuery, CommandSpecies.Auto, aryParams);
		} // GetFirst

		public virtual SafeReader GetFirst(string sQuery, params QueryParameter[] aryParams) {
			return GetFirst(null, sQuery, CommandSpecies.Auto, aryParams);
		} // GetFirst

		public virtual SafeReader GetFirst(string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams) {
			return GetFirst(null, sQuery, nSpecies, aryParams);
		} // GetFirst

		public virtual SafeReader GetFirst(ConnectionWrapper oConnectionToUse, string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams) {
			SafeReader oResult = null;

			ForEachRowSafe(
				oConnectionToUse,
				(sr, bRowsetStart) => {
					oResult = sr.ToCache();
					return ActionResult.SkipAll;
				},
				sQuery,
				nSpecies,
				aryParams
			);

			return oResult ?? SafeReader.CreateEmpty();
		} // GetFirst

		public virtual ConnectionWrapper GetPersistent() {
			var pc = new PooledConnection {
				IsPooled = false,
				Connection = CreateConnection()
			};

			lock (ms_oFreeConnectionLock)
				pc.PoolItemID = ++ms_nFreeConnectionGenerator;

			Log.Debug("A non-pooled connection {0} has been created.", pc.Name);

			return new ConnectionWrapper(pc).Open();
		} // GetPersistent

		public virtual ConnectionWrapper GetPersistentTransaction() {
			return GetPersistent().BeginTransaction();
		} // GetPersistentTransaction

		public ConnectionWrapper TakeFromPool() {
			PooledConnection pc = ms_oPool.Give();

			if (pc == null)
				throw new NullReferenceException("Cannot create a DB connection.");

			if (pc.Connection == null)
				pc.Connection = CreateConnection();

			// uint nReminder = pc.OutOfPoolCount % 10;

			//string sSuffix = "th";

			//switch (nReminder) {
			//case 1:
			//	sSuffix = "st";
			//	break;
			//case 2:
			//	sSuffix = "nd";
			//	break;
			//case 3:
			//	sSuffix = "rd";
			//	break;
			//} // switch

			//Log.Debug("An object (i.e. connection) {3}({2}) is taken from the pool for the {0}{1} time.",
			//	pc.OutOfPoolCount,
			//	sSuffix,
			//	pc.PoolItemID,
			//	pc.Name
			//);

			return new ConnectionWrapper(pc);
		} // TakeFromPool

		protected enum ExecMode {
			Scalar,
			Reader,
			NonQuery,
			ForEachRow,
			Enumerable,
		} // enum ExecMode

		protected virtual string ConnectionString {
			get {
				if (m_sConnectionString != null)
					return m_sConnectionString;

				try {
					m_sConnectionString = ConfigurationManager.ConnectionStrings[Env.Context.ToLower()].ConnectionString;
				} catch (Exception e) {
					string sMsg = string.Format(
						"Failed to load connection string from configuration file using name {0}",
						Env.Context.ToLower()
					);

					Log.Error(sMsg + ": " + e.Message);

					throw new Ezbob.Database.DbException(sMsg, e);
				} // try

				Log.Info("ConnectionString: {0}", m_sConnectionString);

				return m_sConnectionString;
			} // get
			set {
				m_sConnectionString = value;
			} // set
		} // ConnectionString

		protected AConnection(ASafeLog log = null, string sConnectionString = null) {
			Log = log.Safe();
			Env = new Ezbob.Context.Environment(log);
			m_sConnectionString = sConnectionString;
			m_nLogVerbosityLevel = LogVerbosityLevel.Compact;
			m_nCommandTimeout = 30;

			ms_oPool.Log.SetInternal(log);
		} // constructor

		protected AConnection(Ezbob.Context.Environment oEnv, ASafeLog log = null) {
			Log = log.Safe();
			Env = oEnv;
			m_sConnectionString = null;
			m_nLogVerbosityLevel = LogVerbosityLevel.Compact;

			ms_oPool.Log.SetInternal(log);
		} // constructor

		protected abstract void AppendParameter(DbCommand cmd, QueryParameter prm);

		protected virtual DbCommand BuildCommand(string spName, CommandSpecies nSpecies, params QueryParameter[] aryParams) {
			DbCommand command = CreateCommand(spName);

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

			command.CommandTimeout = CommandTimeout;

			foreach (var prm in aryParams)
				AppendParameter(command, prm);

			return command;
		} // BuildCommand

		protected virtual ASafeLog Log { get; private set; }

		protected abstract DbCommand CreateCommand(string sCommand);

		protected abstract DbConnection CreateConnection();

		protected abstract ARetryer CreateRetryer();

		protected void PublishRunningTime(
			string sPooledConnectionID,
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

				Log.Debug("Query {0} in {1}{2} on connection {5}. Request: {3}{4}", sMsg, sTime, sAuxMsg, sSpName, sArgsForLog, sPooledConnectionID);
				break;

			case LogVerbosityLevel.Verbose:
				Log.Debug("Query {1} {2} in {0}ms{3} since query start on connection {4}.", nCurStopwatchValue, guid, sMsg, sAuxMsg, sPooledConnectionID);
				break;

			default:
				throw new ArgumentOutOfRangeException("nLogVerbosityLevel");
			} // switch
		} // PublishRunningTime

		protected virtual object Run(
			ConnectionWrapper cw,
			ExecMode nMode,
			CommandSpecies nSpecies,
			string spName,
			params QueryParameter[] aryParams
		) {
			return Run(cw, null, nMode, nSpecies, spName, aryParams);
		} // Run

		protected virtual object Run(
			ConnectionWrapper cw,
			Func<DbDataReader, bool, ActionResult> oAction,
			ExecMode nMode,
			CommandSpecies nSpecies,
			string spName,
			params QueryParameter[] aryParams
		) {
			if ((nMode == ExecMode.ForEachRow) && ReferenceEquals(oAction, null))
				throw new DbException("Callback action not specified in 'ForEachRow' call.");

			var oArgsForLog = new StringBuilder();

			LogVerbosityLevel nLogVerbosityLevel = LogVerbosityLevel;

			foreach (QueryParameter prm in aryParams)
				oArgsForLog.Append(oArgsForLog.Length > 0 ? ", " : string.Empty).Append(prm);

			string sArgsForLog = "(" + oArgsForLog + ")";

			Guid guid = Guid.NewGuid();

			if (nLogVerbosityLevel == LogVerbosityLevel.Verbose)
				Log.Debug("Starting to run query:\n\tid = {0}\n\t{1}{2}", guid, spName, sArgsForLog);

			ARetryer oRetryer = CreateRetryer();
			oRetryer.LogVerbosityLevel = this.LogVerbosityLevel;

			DbCommand oCmdToDispose = null;

			try {
				DbCommand cmd = BuildCommand(spName, nSpecies, aryParams);
				oCmdToDispose = cmd;

				object oResult = null;

				oRetryer.Retry(() =>
					oResult = RunOnce(cw, oAction, nMode, cmd, nLogVerbosityLevel, spName, sArgsForLog, guid)
				); // Retry

				return oResult;
			} catch (Exception e) {
				if (nLogVerbosityLevel == LogVerbosityLevel.Verbose)
					Log.Error(e, "Error while executing query {0}", guid);
				else
					Log.Error(e, "Error while executing query:\n\tid = {0}\n\t{1}{2}", guid, spName, sArgsForLog);

				throw;
			} finally {
				if (oCmdToDispose != null) {
					oCmdToDispose.Dispose();

					if (nLogVerbosityLevel == LogVerbosityLevel.Verbose)
						Log.Debug("Command has been disposed.");
				} // if
			} // try
		} // Run

		protected virtual object RunOnce(
			ConnectionWrapper oConnectionToUse,
			Func<DbDataReader, bool, ActionResult> oAction,
			ExecMode nMode,
			DbCommand command,
			LogVerbosityLevel nLogVerbosityLevel,
			string spName,
			string sArgsForLog,
			Guid guid
		) {
			ConnectionWrapper oConnection = null;

			bool bAllesInOrdnung = true;

			bool bDropAfterUse = oConnectionToUse == null;

			try {
				oConnection = oConnectionToUse ?? TakeFromPool();

				if (oConnection == null)
					throw new NullReferenceException("There is no available connection to execute the query.");

				command.Connection = oConnection.Connection;

				if (oConnection.Transaction != null)
					command.Transaction = oConnection.Transaction;

				string sPooledConID = oConnection.Pooled.Name;

				var sw = new Stopwatch();
				sw.Start();

				switch (nMode) {
				case ExecMode.Scalar:
					oConnection.Open();
					object value = command.ExecuteScalar();
					PublishRunningTime(sPooledConID, nLogVerbosityLevel, spName, sArgsForLog, guid, sw);
					return value;

				case ExecMode.Reader:
					oConnection.Open();

					var oReader = command.ExecuteReader();

					long nPrevStopwatchValue = sw.ElapsedMilliseconds;

					if (nLogVerbosityLevel == LogVerbosityLevel.Verbose)
						PublishRunningTime(sPooledConID, nLogVerbosityLevel, spName, sArgsForLog, guid, sw);

					var dataTable = new DataTable();
					dataTable.Load(oReader);

					string sMsg;

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

					PublishRunningTime(sPooledConID, nLogVerbosityLevel, spName, sArgsForLog, guid, sw, nPrevStopwatchValue, sMsg);

					oReader.Close();

					return dataTable;

				case ExecMode.NonQuery:
					oConnection.Open();
					int nResult = command.ExecuteNonQuery();
					string sResult = ((nResult == 0) || (nResult == -1)) ? "no" : nResult.ToString(CultureInfo.InvariantCulture);
					PublishRunningTime(sPooledConID, nLogVerbosityLevel, spName, sArgsForLog, guid, sw, sAuxMsg: string.Format("- {0} row{1} changed", sResult, nResult == 1 ? "" : "s"));
					return nResult;

				case ExecMode.ForEachRow:
					oConnection.Open();
					command.ForEachRow(oAction, () => PublishRunningTime(sPooledConID, nLogVerbosityLevel, spName, sArgsForLog, guid, sw));
					return null;

				case ExecMode.Enumerable:
					return command.ExecuteEnumerable(
						oConnection,
						bDropAfterUse ? this : null,
						() => PublishRunningTime(sPooledConID, nLogVerbosityLevel, spName, sArgsForLog, guid, sw)
					);

				default:
					throw new ArgumentOutOfRangeException("nMode");
				} // switch
			} catch (Exception) {
				bAllesInOrdnung = false;
				throw;
			} finally {
				if (bDropAfterUse && (nMode != ExecMode.Enumerable))
					DisposeAfterOneUsage(bAllesInOrdnung, oConnection);
			} // try
		} // RunOnce

		private static void AddColumn(DataTable tbl, Type oType) {
			bool bIsNullable = TypeUtils.IsNullable(oType);

			tbl.Columns.Add(
				string.Empty,
				bIsNullable ? Nullable.GetUnderlyingType(oType) : oType
			);
		} // AddColumn

		private static readonly object ms_oFreeConnectionLock = new object();
		private static readonly DbConnectionPool ms_oPool = new DbConnectionPool();
		private static ulong ms_nFreeConnectionGenerator = 0;

		private int m_nCommandTimeout;

		private LogVerbosityLevel m_nLogVerbosityLevel;

		private string m_sConnectionString;
	} // AConnection
} // namespace Ezbob.Database
