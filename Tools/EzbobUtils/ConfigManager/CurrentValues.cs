namespace ConfigManager {
	using System;
	using System.Collections.Generic;
	using Ezbob.Context;
	using Ezbob.Database;
	using Ezbob.Logger;
	using log4net;

	public partial class CurrentValues
	{
		private static readonly AConnection db;
		private static readonly SafeILog log;

		#region static constructor

		static CurrentValues() {
			ms_oInstance = null;
			ms_oInstanceLock = new object();

			log4net.Config.XmlConfigurator.Configure();
			log = new SafeILog(LogManager.GetLogger(typeof(CurrentValues)));
			db = new SqlConnection(new Ezbob.Context.Environment(), log);
			Init(db, log);
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
					return m_oData.ContainsKey(nIdx) ? m_oData[nIdx] : null;
				} // lock
			} // get
		} // indexer

		#endregion indexer

		#endregion public

		#region protected

		#region constructor

		protected CurrentValues(AConnection oDB, ASafeLog oLog) {
			m_oData = new SortedDictionary<Variables, VariableValue>();

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

					if (Enum.TryParse<Variables>(sName, out nVar))
						m_oData[nVar] = new VariableValue(sr["Value"]);
					else
						Log.Alert("Unknown configuration variable detected: {0}", sName);

					return ActionResult.Continue;
				},
				"GetAllConfigurationVariables",
				CommandSpecies.StoredProcedure
			);
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

		private readonly SortedDictionary<Variables, VariableValue> m_oData;

		private static CurrentValues ms_oInstance;
		private static readonly object ms_oInstanceLock;

		#endregion private
	} // class CurrentValues
} // namespace ConfigManager
