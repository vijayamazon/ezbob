namespace EzBob.Web.Code.ServerLog {
	using System.Collections.Generic;
	using Ezbob.Logger;

	public class ServerLogCachePkgModel {
		public string id { get; set; }
		public List<ServerLogEntryModel> data { get; set; }

		public void Save(ASafeLog oLog) {
			oLog.Debug("FROM CLIENT log package {0} - begin", id);

			if (data != null)
				foreach (var oEvt in data)
					oEvt.Write(oLog);

			oLog.Debug("FROM CLIENT log package {0} - end", id);
		} // Save

	} // class ServerLogCachePkgModel

} // namespace
