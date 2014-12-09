namespace ConfigManager {
	using System;
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Security;
	using JetBrains.Annotations;

	public partial class CurrentValues {

		static CurrentValues() {
			ms_oInstance = null;
			ms_oInstanceLock = new object();
		} // static constructor

		public delegate void ReloadOnTimer();

		public static event ReloadOnTimer OnReloadByTimer;

		public static void Init(AConnection oDB, ASafeLog oLog, UploadLimitations.OnLoadDelegate oUploadLimitationsOnLoad = null) {
			if (ms_oInstance != null)
				return;

			lock (ms_oInstanceLock) {
				if (ms_oInstance != null)
					return;

				ms_oInstance = new CurrentValues(oDB, oLog, oUploadLimitationsOnLoad);
				ms_oInstance.ReLoad();
			} // lock
		} // Init

		public static void ReInit() {
			lock (ms_oInstanceLock) {
				ms_oInstance.ReLoad();
			} // lock
		} // ReInit

		public static CurrentValues Instance {
			get { return ms_oInstance; }
		} // Instance

		public virtual UploadLimitations GetUploadLimitations() {
			bool bReloaded;
			UploadLimitations oResult;

			lock (ms_oInstanceLock) {
				bReloaded = ReloadIfNeeded();
				oResult = m_oUploadLimitations;
			} // lock

			if (bReloaded && (OnReloadByTimer != null))
				OnReloadByTimer();

			return oResult;
		} // GetUploadLimitations

		public virtual OneUploadLimitation GetUploadLimitations(string sControllerName, string sActionName) {
			bool bReloaded;
			OneUploadLimitation oResult;

			lock (ms_oInstanceLock) {
				bReloaded = ReloadIfNeeded();
				oResult = m_oUploadLimitations[sControllerName, sActionName];
			} // lock

			if (bReloaded && (OnReloadByTimer != null))
				OnReloadByTimer();

			return oResult;
		} // GetUploadLimitations

		public virtual VariableValue GetByID(int nID) {
			bool bReloaded;
			VariableValue oResult;

			lock (ms_oInstanceLock) {
				bReloaded = ReloadIfNeeded();
				oResult = m_oByID.ContainsKey(nID) ? m_oByID[nID] : null;
			} // lock

			if (bReloaded && (OnReloadByTimer != null))
				OnReloadByTimer();

			return oResult;
		} // GetByID

		public virtual VariableValue this[Variables nIdx] {
			get {
				bool bReloaded;
				VariableValue oResult;

				lock (ms_oInstanceLock) {
					bReloaded = ReloadIfNeeded();
					oResult = UnsafeGet(nIdx);
				} // lock

				if (bReloaded && (OnReloadByTimer != null))
					OnReloadByTimer();

				return oResult;
			} // get
		} // indexer

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

		public virtual CurrentValues SetDefault(Variables nName, object oValue) {
			lock (ms_oInstanceLock) {
				m_oDefaults[nName] = new VariableValue(0, nName, (oValue ?? "").ToString(), "", Log);
			} // lock

			return this;
		} // SetDefault

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

		protected CurrentValues(AConnection oDB, ASafeLog oLog, UploadLimitations.OnLoadDelegate oUploadLimitationsOnLoad) {
			m_oLastReloadTime = new DateTime();
			m_nRefreshIntervalMinutes = 0;

			m_oByID = new SortedDictionary<int, VariableValue>();
			m_oData = new SortedDictionary<Variables, VariableValue>();
			m_oDefaults = new SortedDictionary<Variables, VariableValue>();

			DB = oDB;
			Log = oLog ?? new SafeLog();

			m_oUploadLimitations = new UploadLimitations(oDB, Log);

			if (oUploadLimitationsOnLoad != null)
				m_oUploadLimitations.OnLoad += oUploadLimitationsOnLoad;
		} // constructor

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

		protected virtual AConnection DB { get; private set; }

		protected ASafeLog Log { get; private set; }

		public virtual VariableValue UpdateOne(Variables nName, string sValue) {
			bool bReloaded;
			VariableValue oResult = null;

			lock (ms_oInstanceLock) {
				bReloaded = ReloadIfNeeded();

				VariableValue v = m_oData.ContainsKey(nName) ? m_oData[nName] : null;

				if (v != null) {
					v.Update(sValue);
					oResult = v.ID > 0 ? v : null;
				} // if
			} // lock

			if (bReloaded && (OnReloadByTimer != null))
				OnReloadByTimer();

			return oResult;
		} // UpdateOne

		private bool ReloadIfNeeded() {
			if (m_nRefreshIntervalMinutes <= 0)
				return false;

			double nAge = (DateTime.UtcNow - m_oLastReloadTime).TotalMinutes;

			if (nAge > int.MaxValue) {
				ReLoad();
				return true;
			}

			if (nAge > m_nRefreshIntervalMinutes) {
				ReLoad();
				return true;
			} // if

			return false;
		} // ReloadIfNeeded

		private VariableValue UnsafeGet(Variables nIdx) {
			if (m_oData.ContainsKey(nIdx))
				return m_oData[nIdx];

			return m_oDefaults.ContainsKey(nIdx) ? m_oDefaults[nIdx] : null;
		} // UnsafeGet

		private readonly SortedDictionary<int, VariableValue> m_oByID;
		private readonly SortedDictionary<Variables, VariableValue> m_oData;
		private readonly SortedDictionary<Variables, VariableValue> m_oDefaults;

		private DateTime m_oLastReloadTime;

		private readonly UploadLimitations m_oUploadLimitations;

		private static CurrentValues ms_oInstance;
		private static readonly object ms_oInstanceLock;

		private class CfgValueUpdate {
			[UsedImplicitly]
			public int ID { get; set; }

			[UsedImplicitly]
			public string Value { get; set; }
		} // CfgValueUpdate

		private class SpUpdateConfigurationVariables : AStoredProc {
			public SpUpdateConfigurationVariables(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {} // constructor

			public override bool HasValidParameters() {
				return (UpdatePackage != null) && (UpdatePackage.Count > 0);
			} // HasValidParameters

			[UsedImplicitly]
			public List<CfgValueUpdate> UpdatePackage { get; set; }
		} // class SpUpdateConfigurationVariables

	} // class CurrentValues
} // namespace ConfigManager
