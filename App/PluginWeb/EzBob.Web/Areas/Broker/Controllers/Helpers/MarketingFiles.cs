namespace EzBob.Web.Areas.Broker.Controllers.Helpers {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.Models;
	using Ezbob.Logger;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;

	internal class MarketingFiles {
		public MarketingFiles(ServiceClient oServiceClient, int originID) {
			IEnumerable<FileDescription> oFiles = Load(oServiceClient, originID);

			this.alphabetical = new SortedDictionary<string, FileDescription>();

			foreach (FileDescription fd in oFiles)
				this.alphabetical[fd.FileID] = fd;
		} // constructor

		public FileDescription Find(string sKey) {
			if (this.alphabetical == null)
				return null;

			return this.alphabetical.ContainsKey(sKey) ? this.alphabetical[sKey] : null;
		} // Find

		private readonly SortedDictionary<string, FileDescription> alphabetical;

		private IEnumerable<FileDescription> Load(ServiceClient oServiceClient, int originID) {
			log.Debug("Loading broker marketing files...");

			BrokerStaticDataActionResult flar = null;

			try {
				flar = oServiceClient.Instance.BrokerLoadStaticData(true, originID);
			} catch (Exception e) {
				log.Alert(e, "Failed to load broker marketing files.");
			} // try

			FileDescription[] oResult = (flar == null ? null : flar.Files) ?? new FileDescription[0];

			log.Debug("Loading broker marketing files complete, {0} file(s) loaded.", oResult.Length);

			return oResult;
		} // Load

		private static readonly ASafeLog log = new SafeILog(typeof(MarketingFiles));
	} // class MarketingFiles
} // namespace
