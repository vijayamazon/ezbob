namespace ConfigManager {
	using System;
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Microsoft.Web.Administration;

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
			m_oLimits.Clear();

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

			if (OnLoad != null)
				OnLoad(this);
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

		#region method UpdateWebSiteCfg

		public void UpdateWebSiteCfg(string sWebSiteName) {
			var oPendingUpdate = new List<Tuple<string, int>>();

			using (ServerManager oServerManager = new ServerManager()) {
				Microsoft.Web.Administration.Configuration oConfig = oServerManager.GetWebConfiguration(sWebSiteName);

				int nFileSize = GetWebSiteUploadLimitation(oConfig, null);

				if (nFileSize != Global.FileSize)
					oPendingUpdate.Add(new Tuple<string, int>(null, Global.FileSize));

				foreach (var pair in m_oLimits) {
					nFileSize = GetWebSiteUploadLimitation(oConfig, pair.Key);

					if (nFileSize != pair.Value.FileSize)
						oPendingUpdate.Add(new Tuple<string, int>(pair.Key, pair.Value.FileSize));
				} // for each

				if (oPendingUpdate.Count > 0) {
					m_oLog.Debug("{0} upload limit{1} to update.", oPendingUpdate.Count, oPendingUpdate.Count == 1 ? string.Empty : "s");

					foreach (var pt in oPendingUpdate)
						SetWebSiteUploadLimitation(oConfig, pt.Item2, pt.Item1);

					oServerManager.CommitChanges();

					m_oLog.Debug("Upload limit changes have been committed.");
				}
				else
					m_oLog.Debug("No upload limits to update.");
			} // using
		} // UpdateWebSiteCfg

		#endregion method UpdateWebSiteCfg

		public delegate void OnLoadDelegate(UploadLimitations oLimitations);

		public event OnLoadDelegate OnLoad;

		#endregion public

		#region private

		private readonly SortedDictionary<string, OneUploadLimitation> m_oLimits;
		private readonly AConnection m_oDB;
		private readonly ASafeLog m_oLog;

		private const string SectionPath = "system.webServer/security/requestFiltering";
		private const string ElementName = "requestLimits";
		private const string AttributeName = "maxAllowedContentLength";

		#region method SetWebSiteUploadLimitation

		private void SetWebSiteUploadLimitation(Microsoft.Web.Administration.Configuration oConfig, int nUploadLimit, string sControllerAction) {
			try {
				ConfigurationSection requestFilteringSection = string.IsNullOrWhiteSpace(sControllerAction)
					? oConfig.GetSection(SectionPath)
					: oConfig.GetSection(SectionPath, sControllerAction);

				ConfigurationElement requestLimitsElement = requestFilteringSection.GetChildElement(ElementName);

				requestLimitsElement.SetAttributeValue(AttributeName, nUploadLimit);

				m_oLog.Debug("Updated upload limit to '{0}' for action '{1}'.", nUploadLimit, sControllerAction);
			}
			catch (Exception e) {
				m_oLog.Warn(e, "Failed to update upload limit to '{0}' for action '{1}'.", nUploadLimit, sControllerAction);
			} // try
		} // SetWebSiteUploadLimitation

		#endregion method SetWebSiteUploadLimitation

		#region method GetWebSiteUploadLimitation

		private int GetWebSiteUploadLimitation(Microsoft.Web.Administration.Configuration oConfig, string sControllerAction) {
			int nResult = 0;

			try {
				ConfigurationSection requestFilteringSection = string.IsNullOrWhiteSpace(sControllerAction)
					? oConfig.GetSection(SectionPath)
					: oConfig.GetSection(SectionPath, sControllerAction);

				ConfigurationElement requestLimitsElement = requestFilteringSection.GetChildElement(ElementName);

				ConfigurationAttribute oAttr = requestLimitsElement.GetAttribute(AttributeName);

				if (oAttr != null) {
					int macl;

					if (int.TryParse(oAttr.Value.ToString(), out macl))
						nResult = macl;
					else
						m_oLog.Warn("Failed to parse upload limit for action '{0}'.", sControllerAction);
				} // if

				m_oLog.Debug("Current upload limit for action '{1}' is {0} bytes.", nResult, sControllerAction);
			}
			catch (Exception e) {
				m_oLog.Warn(e, "Failed to load upload limit for action '{0}'.", sControllerAction);
			} // try

			return nResult;
		} // GetWebSiteUploadLimitation

		#endregion method GetWebSiteUploadLimitation

		#endregion private
	} // class UploadLimitations
} // namespace
