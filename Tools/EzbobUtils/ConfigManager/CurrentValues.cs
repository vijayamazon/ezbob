namespace ConfigManager {
	using System;
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Security;
	using JetBrains.Annotations;

	public partial class CurrentValues {
		#region static constructor

		static CurrentValues() {
			ms_oInstance = null;
			ms_oInstanceLock = new object();
		} // static constructor

		#endregion static constructor

		#region public

		#region singleton

		#region method Init

		public static void Init(AConnection oDB, ASafeLog oLog) {
			if (ms_oInstance != null)
				return;

			lock (ms_oInstanceLock) {
				if (ms_oInstance != null)
					return;

				ms_oInstance = new CurrentValues(oDB, oLog);
				ms_oInstance.ReLoad();
			} // lock
		} // Init

		#endregion method Init

		#region method ReInit

		public static void ReInit() {
			lock (ms_oInstanceLock) {
				ms_oInstance.ReLoad();
			} // lock
		} // ReInit

		#endregion method ReInit

		#region property Instance

		public static CurrentValues Instance {
			get { return ms_oInstance; }
		} // Instance

		#endregion property Instance

		#endregion singleton

		#region method GetUploadLimitations

		public virtual UploadLimitations GetUploadLimitations(int nID) {
			lock (ms_oInstanceLock) {
				ReloadIfNeeded();
				return m_oUploadLimitations;
			} // lock
		} // GetUploadLimitations

		#endregion method GetUploadLimitations

		#region method GetByID

		public virtual VariableValue GetByID(int nID) {
			lock (ms_oInstanceLock) {
				ReloadIfNeeded();
				return m_oByID.ContainsKey(nID) ? m_oByID[nID] : null;
			} // lock
		} // GetByID

		#endregion method GetByID

		#region indexer

		public virtual VariableValue this[Variables nIdx] {
			get {
				lock (ms_oInstanceLock) {
					ReloadIfNeeded();
					return UnsafeGet(nIdx);
				} // lock
			} // get
		} // indexer

		#endregion indexer

		#region property RefreshIntervalMinutes

		public virtual int RefreshIntervalMinutes {
			get {
				lock (ms_oInstanceLock) {
					return m_nRefreshIntervalMinutes;
				} // lock
			} // get

			set {
				lock (ms_oInstanceLock) {
					m_nRefreshIntervalMinutes = Math.Max(0, value);
				} // lock
			} // set
		} // RefreshIntervalMinutes

		private int m_nRefreshIntervalMinutes;

		#endregion property RefreshInterval

		#region method SetDefault

		public virtual CurrentValues SetDefault(Variables nName, object oValue) {
			lock (ms_oInstanceLock) {
				m_oDefaults[nName] = new VariableValue(0, nName, (oValue ?? "").ToString(), "", Log);
			} // lock

			return this;
		} // SetDefault

		#endregion method SetDefault

		#region method Update

		public void Update(SortedDictionary<Variables, string> oPackage) {
			if (oPackage == null)
				return;

			if (oPackage.Count < 1)
				return;

			var sp = new SpUpdateConfigurationVariables(DB, Log) {
				UpdatePackage = new List<CfgValueUpdate>(),
			};

			foreach (var pair in oPackage) {
				VariableValue oValue = UpdateOne(pair.Key, pair.Value);

				if (oValue != null)
					sp.UpdatePackage.Add(new CfgValueUpdate { ID = oValue.ID, Value = oValue.Value, });
			} // for each

			sp.ExecuteNonQuery();
		} // SetByName

		#endregion method Update

		#endregion public

		#region protected

		#region constructor

		protected CurrentValues(AConnection oDB, ASafeLog oLog) {
			m_oLastReloadTime = new DateTime();
			m_nRefreshIntervalMinutes = 0;

			m_oByID = new SortedDictionary<int, VariableValue>();
			m_oData = new SortedDictionary<Variables, VariableValue>();
			m_oDefaults = new SortedDictionary<Variables, VariableValue>();

			DB = oDB;
			Log = oLog ?? new SafeLog();

			m_oUploadLimitations = new UploadLimitations(oDB, Log);
		} // constructor

		#endregion constructor

		#region method ReLoad

		protected virtual void ReLoad() {
			m_oData.Clear();
			m_oByID.Clear();

			DB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					Variables nVar;
					string sName = sr["Name"];

					if (Enum.TryParse(sName, out nVar)) {
						string sValue = sr["Value"];

						if (sr["IsEncrypted"]) {
							try {
								sValue = Encrypted.Decrypt(sValue);
							}
							catch (Exception e) {
								Log.Alert(e, "Failed to decrypt a value of {0}.", nVar);
							} // try
						} // if

						var vv = new VariableValue(sr["ID"], nVar, sValue, sr["Description"], Log);

						m_oData[vv.Name] = vv;
						m_oByID[vv.ID] = vv;
					}
					else
						Log.Warn("Unknown configuration variable detected: {0}", sName);

					return ActionResult.Continue;
				},
				"GetAllConfigurationVariables",
				CommandSpecies.StoredProcedure
			);

			m_oLastReloadTime = DateTime.UtcNow;

			VariableValue.LogVerbosityLevel = UnsafeGet(Variables.VerboseConfigurationLogging)
				? LogVerbosityLevel.Verbose
				: LogVerbosityLevel.Compact;

			m_oUploadLimitations.Load();
		} // ReLoad

		#endregion method ReLoad

		#region property DB

		protected virtual AConnection DB { get; private set; }

		#endregion property DB

		#region property Log

		protected ASafeLog Log { get; private set; }

		#endregion property Log

		#endregion protected

		#region private

		#region method UpdateOne

		public virtual VariableValue UpdateOne(Variables nName, string sValue) {
			lock (ms_oInstanceLock) {
				ReloadIfNeeded();

				VariableValue v = m_oData.ContainsKey(nName) ? m_oData[nName] : null;

				if (v != null) {
					v.Update(sValue);
					return v.ID > 0 ? v : null;
				} // if

				return null;
			} // lock
		} // UpdateOne

		#endregion method UpdateOne

		#region method ReloadIfNeeded

		private void ReloadIfNeeded() {
			if (m_nRefreshIntervalMinutes <= 0)
				return;

			double nAge = (DateTime.UtcNow - m_oLastReloadTime).TotalMinutes;

			if (nAge > int.MaxValue)
				ReLoad();
			else if (nAge > m_nRefreshIntervalMinutes)
				ReLoad();
		} // ReloadIfNeeded

		#endregion method ReloadIfNeeded

		#region method UnsafeGet

		private VariableValue UnsafeGet(Variables nIdx) {
			if (m_oData.ContainsKey(nIdx))
				return m_oData[nIdx];

			return m_oDefaults.ContainsKey(nIdx) ? m_oDefaults[nIdx] : null;
		} // UnsafeGet

		#endregion method UnsafeGet

		#region fields

		private readonly SortedDictionary<int, VariableValue> m_oByID;
		private readonly SortedDictionary<Variables, VariableValue> m_oData;
		private readonly SortedDictionary<Variables, VariableValue> m_oDefaults;

		private DateTime m_oLastReloadTime;

		private UploadLimitations m_oUploadLimitations;

		private static CurrentValues ms_oInstance;
		private static readonly object ms_oInstanceLock;

		#endregion fields

		#region class CfgValueUpdate

		private class CfgValueUpdate {
			[UsedImplicitly]
			public int ID { get; set; }

			[UsedImplicitly]
			public string Value { get; set; }
		} // CfgValueUpdate

		#endregion class CfgValueUpdate

		#region class SpUpdateConfigurationVariables

		private class SpUpdateConfigurationVariables : AStoredProc {
			public SpUpdateConfigurationVariables(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {} // constructor

			public override bool HasValidParameters() {
				return (UpdatePackage != null) && (UpdatePackage.Count > 0);
			} // HasValidParameters

			[UsedImplicitly]
			public List<CfgValueUpdate> UpdatePackage { get; set; }
		} // class SpUpdateConfigurationVariables

		#endregion class SpUpdateConfigurationVariables

		#endregion private
	} // class CurrentValues
} // namespace ConfigManager
