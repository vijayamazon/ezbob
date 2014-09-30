namespace Reports {
	using System;
	using System.Globalization;
	using DropNet;
	using DropNet.Exceptions;
	using DropNet.Models;
	using Ezbob.Database;
	using Ezbob.Logger;

	#region class DropboxReportSaver

	class DropboxReportSaver : SafeLog {
		#region public

		#region constructor

		public DropboxReportSaver(AConnection oDB, ASafeLog log = null) : base(log) {
			DB = oDB;
		} // constructor

		#endregion constructor

		#region method Init

		public virtual bool Init() {
			Info("Loading Dropbox configuration...");

			DB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					string sName = sr["Name"];
					string sValue = sr["Value"];

					ConfigurationVariables cv = ConfigurationVariables.ZeroIsIgnored;

					if (ConfigurationVariables.TryParse(sName, out cv)) {
						switch (cv) {
						case ConfigurationVariables.ZeroIsIgnored:
							break;

						case ConfigurationVariables.ReportDaemonDropboxCredentials:
							string[] ary = sValue.Trim().Split(';');

							if ((ary == null) || (ary.Length != 4))
								throw new Exception("Dropbox credentials not specified.");

							m_sAppKey     = ary[0];
							m_sAppSecret  = ary[1];
							m_sUserKey    = ary[2];
							m_sUserSecret = ary[3];

							break;

						case ConfigurationVariables.ReportDaemonDropboxRootPath:
							m_sRootPath = sValue.Trim();

							while (m_sRootPath.EndsWith("/") )
								m_sRootPath = m_sRootPath.Remove(m_sRootPath.Length - 1);

							if (string.IsNullOrWhiteSpace(m_sRootPath))
								throw new Exception("Dropbox root path not specified.");

							break;

						default:
							throw new ArgumentOutOfRangeException("Unimplemented configuration parameter: " + cv.ToString());
						} // switch
					} // if

					return ActionResult.Continue;
				},
				"SELECT Name, Value FROM ConfigurationVariables WHERE Name LIKE 'ReportDaemonDropbox%'",
				CommandSpecies.Text
			);

			Debug("*********************************************************");
			Debug("***");
			Debug("*** Dropbox Report Saver Configuration - begin");
			Debug("***");
			Debug("*********************************************************");

			Debug("Application: {0} - {1}", m_sAppKey, m_sAppSecret);
			Debug("User: {0} - {1}", m_sUserKey, m_sUserSecret);
			Debug("Root path: {0}", m_sRootPath);

			Debug("*********************************************************");
			Debug("***");
			Debug("*** Dropbox Report Saver Configuration - end");
			Debug("***");
			Debug("*********************************************************");

			Info("Loading Dropbox configration complete.");
			return true;
		} // Init

		#endregion method Init

		#region method Send

		public virtual void Send(string sReportName, DateTime oReportDate, string sFileExtension, byte[] oReportBody) {
			const int nRetryCount = 5;

			Debug("Sending {0} report to Dropbox...", sReportName);

			var client = new DropNetClient(m_sAppKey, m_sAppSecret, m_sUserKey, m_sUserSecret);

			string sDir = m_sRootPath + "/" + sReportName.Replace(" ", "_");

			MetaData meta = null;

			for (int i = 1; i <= nRetryCount; i++) {
				Debug("Looking for the target directory {0}, attempt #{1}...", sDir, i);

				try {
					meta = client.GetMetaData(sDir);
					break;
				}
				catch (DropboxException e) {
					Warn("Exception caught: {0}", e.Message);
					Warn("Status code: {0}", e.StatusCode.ToString());
					Warn("Response: {0}", e.Response.Content);

					if (i < nRetryCount)
						Debug("Retrying to look for the target directory.");
				} // try
			} // for

			if (meta == null) {
				Debug("The target directory not found, creating...");

				try {
					meta = client.CreateFolder(sDir);
				}
				catch (DropboxException e) {
					Warn("Exception caught: {0}", e.Message);
					Warn("Status code: {0}", e.StatusCode.ToString());
					Warn("Response: {0}", e.Response.Content);
				} // try
			}
			else
				Debug("The target dir already exists.");

			if (meta == null)
				throw new Exception("Failed to find/create target directory " + sDir);

			var sFileName = string.Format(
				"{0}.{1}",
				oReportDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
				sFileExtension
			);

			Debug("Uploading {0} to {1}...", sFileName, sDir);

			client.UploadFile(sDir, sFileName, oReportBody);

			Debug("Uploading {0} to {1} complete.", sFileName, sDir);

			Debug("Sending {0} report to Dropbox complete.", sReportName);
		} // Send

		#endregion method Send

		#endregion public

		#region protected

		protected virtual AConnection DB { get; private set; }

		#endregion protected

		#region private

		private string m_sAppKey;
		private string m_sAppSecret;
		private string m_sUserKey;
		private string m_sUserSecret;
		private string m_sRootPath;

		private enum ConfigurationVariables {
			ZeroIsIgnored,
			ReportDaemonDropboxCredentials,
			ReportDaemonDropboxRootPath,
		} // enum ConfigurationVariables

		#endregion private
	} // class DropboxReportSaver

	#endregion class DropboxReportSaver
} // namespace Reports
