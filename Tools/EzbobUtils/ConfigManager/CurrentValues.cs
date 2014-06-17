namespace ConfigManager {
	using System;
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Security;

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

		#region indexer

		public virtual VariableValue this[Variables nIdx] {
			get {
				lock (ms_oInstanceLock) {
					if (m_nRefreshIntervalMinutes > 0) {
						double nAge = (DateTime.UtcNow - m_oLastReloadTime).TotalMinutes;

						if (nAge > int.MaxValue)
							ReLoad();
						else if (nAge > m_nRefreshIntervalMinutes)
							ReLoad();
					} // if

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
				m_oDefaults[nName] = new VariableValue(nName, (oValue ?? "").ToString(), Log);
			} // lock

			return this;
		} // SetDefault

		#endregion method SetDefault

		#endregion public

		#region protected

		#region constructor

		protected CurrentValues(AConnection oDB, ASafeLog oLog) {
			m_oLastReloadTime = new DateTime();
			m_nRefreshIntervalMinutes = 0;
			m_oData = new SortedDictionary<Variables, VariableValue>();
			m_oDefaults = new SortedDictionary<Variables, VariableValue>();

			DB = oDB;
			Log = new SafeLog(oLog);
		} // constructor

		#endregion constructor

		#region method ReLoad

		protected virtual void ReLoad() {
			m_oData.Clear();

			DB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					Variables nVar;
					string sName = sr["Name"];

					if (Enum.TryParse<Variables>(sName, out nVar)) {
						string sValue = sr["Value"];

						if (sr["IsEncrypted"]) {
							try {
								sValue = Encrypted.Decrypt(sValue);
							}
							catch (Exception e) {
								Log.Alert(e, "Failed to decrypt a value of {0}.", nVar);
							} // try
						} // if

						m_oData[nVar] = new VariableValue(nVar, sValue, Log);
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

		#region method UnsafeGet

		private VariableValue UnsafeGet(Variables nIdx) {
			if (m_oData.ContainsKey(nIdx))
				return m_oData[nIdx];

			return m_oDefaults.ContainsKey(nIdx) ? m_oDefaults[nIdx] : null;
		} // UnsafeGet

		#endregion method UnsafeGet

		private readonly SortedDictionary<Variables, VariableValue> m_oData;
		private readonly SortedDictionary<Variables, VariableValue> m_oDefaults;

		private DateTime m_oLastReloadTime;

		private static CurrentValues ms_oInstance;
		private static readonly object ms_oInstanceLock;

		#endregion private
	} // class CurrentValues
} // namespace ConfigManager
