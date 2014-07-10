namespace Ezbob.Database {
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Data.Common;
	using System.Text;
	using System.Data;
	using System.Diagnostics;
	using System.Configuration;
	using System.Globalization;

	using Ezbob.Logger;
	using Utils;

	public abstract class AConnection : SafeLog {
		#region public

		#region method OpenPersistent

		public virtual void OpenPersistent() {
			if (PersistentConnection != null)
				return;

			PersistentConnection = new ConnectionWrapper(CreateConnection(), true);
		} // OpenPersistent

		#endregion method OpenPersistent

		#region method ClosePersistent

		public virtual void ClosePersistent() {
			if (PersistentConnection == null)
				return;

			if (PersistentConnection.IsOpen)
				PersistentConnection.Connection.Dispose();

			PersistentConnection = null;
		} // ClosePersistent

		#endregion method ClosePersistent

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

		[Obsolete]
		public DataTable ExecuteReader(string sQuery, params QueryParameter[] aryParams) {
			return ExecuteReader(sQuery, CommandSpecies.Auto, aryParams);
		} // ExecuteReader

		[Obsolete]
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

		#region method ForEachResult

		public virtual void ForEachResult<T>(Func<T, ActionResult> oAction, string sQuery, params QueryParameter[] aryParams) where T : IResultRow, new() {
			ForEachResult<T>(oAction, sQuery, CommandSpecies.Auto, aryParams);
		} // ForEachResult

		public virtual void ForEachResult<T>(Func<T, ActionResult> oAction, string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams) where T : IResultRow, new() {
			if (ReferenceEquals(oAction, null))
				throw new DbException("Callback action not specified in 'ForEachResult' call.");

			ForEachRowSafe(
				(sr, bRowsetStart) => {
					var oResult = new T();
					oResult.SetIsFirst(bRowsetStart);

					sr.Fill(oResult);

					return oAction(oResult);
				},
				sQuery,
				nSpecies,
				aryParams
			);
		} // ForEachResult

		#endregion method ForEachResult

		#region method Fill

		public List<T> Fill<T>(string sQuery, params QueryParameter[] aryParams) where T: ITraversable, new() {
			return Fill<T>(sQuery, CommandSpecies.Auto, aryParams);
		} // Fill

		public List<T> Fill<T>(string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams) where T : ITraversable, new() {
			var oResult = new List<T>();

			ForEachRowSafe(
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

		#endregion method Fill

		#region method FillFirst

		public T FillFirst<T>(string sQuery, params QueryParameter[] aryParams) where T: ITraversable, new() {
			return FillFirst<T>(sQuery, CommandSpecies.Auto, aryParams);
		} // FillFirst

		public T FillFirst<T>(string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams) where T : ITraversable, new() {
			var oResult = new T();

			ForEachRowSafe(
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

		public virtual void FillFirst<T>(T oInstatnce, string sQuery, params QueryParameter[] aryParams) where T: ITraversable {
			FillFirst<T>(oInstatnce, sQuery, CommandSpecies.Auto, aryParams);
		} // FillFirst

		public virtual void FillFirst<T>(T oInstance, string sQuery, CommandSpecies nSpecies, params QueryParameter[] aryParams) where T : ITraversable {
			ForEachRowSafe(
				(sr, bRowsetStart) => {
					sr.Fill(oInstance);
					return ActionResult.SkipAll;
				},
				sQuery,
				nSpecies,
				aryParams
			);
		} // FillFirst

		#endregion method FillFirst

		#region method CreateVectorParameter

		public virtual QueryParameter CreateVectorParameter<T>(string sFieldName, params T[] oValues) {
			return CreateVectorParameter<T>(sFieldName, (IEnumerable<T>)oValues);
		} // CreateVectorParameter

		public class VectorToTableParameter<T> : ITraversable {
			public T Value { get; set; } // Value
		} // class VectorToTableParameter

		public virtual QueryParameter CreateVectorParameter<T>(string sFieldName, IEnumerable<T> oValues) {
			return CreateTableParameter<VectorToTableParameter<T>>(sFieldName, oValues, oOneValue => new object[] { oOneValue });
		} // CreateVectorParameter

		#endregion method CreateVectorParameter

		#region method CreateTableParameter

		public virtual QueryParameter CreateTableParameter<TColumnInfo, TSource>(string sFieldName, IEnumerable<TSource> oValues, ParameterDirection nDirection = ParameterDirection.Input)
			where TColumnInfo : ITraversable, new()
			where TSource : ITraversable
		{
			return CreateTableParameter<TColumnInfo>(sFieldName, oValues, TypeUtils.GetConvertorToObjectArray(typeof(TSource)), nDirection);
		} // CreateTableParameter

		public virtual QueryParameter CreateTableParameter<TColumnInfo>(
			string sFieldName,
			IEnumerable oValues,
			Func<object, object[]> oValueToRow,
			ParameterDirection nDirection = ParameterDirection.Input
		) where TColumnInfo : ITraversable, new() {
			return CreateTableParameter(typeof (TColumnInfo), sFieldName, oValues, oValueToRow, nDirection);
		} // CreateTableParameter

		public virtual QueryParameter CreateTableParameter(Type oColumnInfo, string sFieldName, IEnumerable oValues, Func<object, object[]> oValueToRow, ParameterDirection nDirection = ParameterDirection.Input) {
			var tbl = new DataTable();

			if (TypeUtils.IsSimpleType(oColumnInfo)) {
				bool bIsNullable = TypeUtils.IsNullable(oColumnInfo);

				tbl.Columns.Add(
					string.Empty,
					bIsNullable ? Nullable.GetUnderlyingType(oColumnInfo) : oColumnInfo
				);
			}
			else {
				PropertyTraverser.Traverse(oColumnInfo, (oIgnoredInstance, oPropertyInfo) => {
					bool bIsNullable = TypeUtils.IsNullable(oPropertyInfo.PropertyType);

					tbl.Columns.Add(
						string.Empty,
						bIsNullable ? Nullable.GetUnderlyingType(oPropertyInfo.PropertyType) : oPropertyInfo.PropertyType
					);
				});
			} // if

			if (oValues != null)
				foreach (object v in oValues)
					tbl.Rows.Add(oValueToRow(v));

			return BuildTableParameter(sFieldName, tbl, nDirection);
		} // CreateTableParameter

		#endregion method CreateTableParameter

		public abstract QueryParameter BuildTableParameter(string sFieldName, DataTable oValues, ParameterDirection nDirection = ParameterDirection.Input);

		public abstract string DateToString(DateTime oDate);

		public abstract DbCommand NewCommand { get; }

		#region property CommandTimeout

		public virtual int CommandTimeout {
			get { return m_nCommandTimeout; }
			set { m_nCommandTimeout = value; }
		} // CommandTimeout

		private int m_nCommandTimeout;

		#endregion property CommandTimeout

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
			m_nCommandTimeout = 30;
			m_oPersistentConnection = null;
		} // constructor

		protected AConnection(Ezbob.Context.Environment oEnv, ASafeLog log = null) : base(log) {
			Env = oEnv;
			m_sConnectionString = null;
			m_nLogVerbosityLevel = LogVerbosityLevel.Compact;
			m_oPersistentConnection = null;
		} // Env

		#endregion constructor

		#region abstract methods

		protected abstract DbConnection CreateConnection();
		protected abstract DbCommand CreateCommand(string sCommand);
		protected abstract void AppendParameter(DbCommand cmd, QueryParameter prm);

		protected abstract ARetryer CreateRetryer();

		#endregion abstract methods

		#region method GetConnection

		protected virtual ConnectionWrapper GetConnection() {
			return PersistentConnection ?? new ConnectionWrapper(CreateConnection(), false);
		} // GetConnection

		#endregion method GetConnection

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

			DbCommand oCmdToDispose = null;

			try {
				DbCommand cmd = BuildCommand(spName, nSpecies, aryParams);
				oCmdToDispose = cmd;

				object oResult = null;

				oRetryer.Retry(() =>
					oResult = RunOnce(oAction, nMode, cmd, nLogVerbosityLevel, spName, sArgsForLog, guid)
				); // Retry

				return oResult;
			}
			catch (Exception e) {
				if (nLogVerbosityLevel == LogVerbosityLevel.Verbose)
					Error(e, "Error while executing query {0}", guid);
				else
					Error(e, "Error while executing query:\n\tid = {0}\n\t{1}{2}", guid, spName, sArgsForLog);

				throw;
			}
			finally {
				if (oCmdToDispose != null) {
					oCmdToDispose.Dispose();

					if (nLogVerbosityLevel == LogVerbosityLevel.Verbose)
						Debug("Command has been disposed.");
				} // if
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

		#region method BuildCommand

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

		#endregion method BuildCommand

		#region method RunOnce

		protected virtual object RunOnce(
			Func<DbDataReader, bool, ActionResult> oAction,
			ExecMode nMode, 
			DbCommand command,
			LogVerbosityLevel nLogVerbosityLevel,
			string spName,
			string sArgsForLog,
			Guid guid
		) {
			using (var cw = GetConnection()) {
				command.Connection = cw.Connection;

				cw.Open();

				var sw = new Stopwatch();
				sw.Start();

				switch (nMode) {
				case ExecMode.Scalar:
					return RunScalar(command, nLogVerbosityLevel, spName, sArgsForLog, guid, sw);

				case ExecMode.Reader:
					return RunReader(command, nLogVerbosityLevel, spName, sArgsForLog, guid, sw);

				case ExecMode.NonQuery:
					return RunNonQuery(command, nLogVerbosityLevel, spName, sArgsForLog, guid, sw);

				case ExecMode.ForEachRow: {
					command.ForEachRow(oAction, () => PublishRunningTime(nLogVerbosityLevel, spName, sArgsForLog, guid, sw));
					return null;
				} // ExecMode.ForEachRow

				default:
					throw new ArgumentOutOfRangeException("nMode");
				} // switch
			} // using connection
		} // RunOnce

		#endregion method RunOnce

		#region method RunScalar

		protected virtual object RunScalar(DbCommand command, LogVerbosityLevel nLogVerbosityLevel, string spName, string sArgsForLog, Guid guid, Stopwatch sw) {
			object value = command.ExecuteScalar();
			PublishRunningTime(nLogVerbosityLevel, spName, sArgsForLog, guid, sw);
			return value;
		} // RunScalar

		#endregion method RunScalar

		#region method RunReader

		protected virtual DataTable RunReader(DbCommand command, LogVerbosityLevel nLogVerbosityLevel, string spName, string sArgsForLog, Guid guid, Stopwatch sw) {
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
		} // RunReader

		#endregion method RunReader

		#region method RunNonQuery

		protected virtual int RunNonQuery(DbCommand command, LogVerbosityLevel nLogVerbosityLevel, string spName, string sArgsForLog, Guid guid, Stopwatch sw) {
			int nResult = command.ExecuteNonQuery();
			string sResult = ((nResult == 0) || (nResult == -1)) ? "no" : nResult.ToString(CultureInfo.InvariantCulture);
			PublishRunningTime(nLogVerbosityLevel, spName, sArgsForLog, guid, sw, sAuxMsg: string.Format("- {0} row{1} changed", sResult, nResult == 1 ? "" : "s"));
			return nResult;
		} // RunNonQuery

		#endregion method RunNonQuery

		#region class ConnectionWrapper

		protected class ConnectionWrapper : IDisposable {
			#region constructor

			public ConnectionWrapper(DbConnection oConnection, bool bIsPersistent) {
				Connection = oConnection;
				IsPersistent = bIsPersistent;
				IsOpen = false;
			} // constructor

			#endregion constructor

			#region method Open

			public void Open() {
				if (IsOpen)
					return;

				if (Connection == null)
					return;

				Connection.Open();

				IsOpen = true;
			} // Open

			#endregion method Open

			#region method Dispose

			public void Dispose() {
				if (!IsPersistent && (Connection != null))
					Connection.Dispose();
			} // Dispose

			#endregion method Dispose

			public DbConnection Connection { get; private set; }

			public bool IsPersistent { get; private set; }

			public bool IsOpen { get; private set; }
		} // class ConnectionWrapper

		#endregion class ConnectionWrapper

		#region property PersistentConnection

		protected virtual ConnectionWrapper PersistentConnection {
			get { return m_oPersistentConnection; }
			set { m_oPersistentConnection = value; }
		} // PersistentConnection

		private ConnectionWrapper m_oPersistentConnection;

		#endregion property PersistentConnection

		#endregion protected
	} // AConnection
} // namespace Ezbob.Database
