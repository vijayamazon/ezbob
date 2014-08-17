namespace ConfigManager {
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class UploadLimitations {
		#region public

		#region constructor

		public UploadLimitations(AConnection oDB, ASafeLog oLog) {
			m_oLimits = new SortedDictionary<string, OneUploadLimitation>();
			Global = new OneUploadLimitation();

			m_oDB = oDB;
			m_oLog = oLog ?? new SafeLog();
		} // constructor

		#endregion constructor

		#region method Load

		public void Load() {
			m_oDB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					string sControllerName = sr["ControllerName"];

					if (string.IsNullOrWhiteSpace(sControllerName))
						Global.Init(sr["FileSizeLimit"], sr["AcceptedFiles"]);
					else {
						int? nFileSize = sr["FileSizeLimit"];
						string sAcceptedFiles = sr["AcceptedFiles"];
						string sKey = sControllerName + "/" + sr["ActionName"];

						m_oLimits[sKey] = new OneUploadLimitation(
							nFileSize ?? Global.FileSize,
							string.IsNullOrWhiteSpace(sAcceptedFiles) ? Global.AcceptedFiles : sAcceptedFiles 
						);
					} // if

					return ActionResult.Continue;
				},
				"LoadUploadLimitations",
				CommandSpecies.StoredProcedure
			);

			m_oLog.Debug("Upload limitations configuration - begin:");
			m_oLog.Debug("Global: {0}", Global);

			foreach (var pair in m_oLimits)
				m_oLog.Debug("{0}: {1}", pair.Key, pair.Value);

			m_oLog.Debug("Upload limitations configuration - end.");
		} // Load

		#endregion method Load

		#region property Global

		public OneUploadLimitation Global { get; private set; }

		#endregion property Global

		#region property indexer

		public OneUploadLimitation this[string sControllerName, string sActionName] {
			get {
				if (string.IsNullOrWhiteSpace(sControllerName) || string.IsNullOrWhiteSpace(sActionName))
					return Global;

				string sKey = sControllerName + "/" + sActionName;

				return m_oLimits.ContainsKey(sKey) ? m_oLimits[sKey] : null;
			} // get
		} // indexer

		#endregion property indexer

		#endregion public

		#region private

		private readonly SortedDictionary<string, OneUploadLimitation> m_oLimits;
		private readonly AConnection m_oDB;
		private readonly ASafeLog m_oLog;

		#endregion private
	} // class UploadLimitations
} // namespace
