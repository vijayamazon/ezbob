namespace EzBob.Web.Code.UiEvents {
	using System.Collections.Generic;
	using System.Text;
	using Ezbob.Database;

	public class UiCachePkgModel {

		public class SaveResult {
			public SaveResult(string sID) {
				id = sID;
				success = new List<string>();
				failcount = 0;
			} // constructor

			public void Add(string sItemID, bool bSuccess) {
				if (bSuccess)
					success.Add(sItemID);
				else
					failcount++;
			} // Add

			public bool Overall() { return failcount < 1; } // Overall

			public List<string> success { get; private set; }
			public string id { get; private set; }
			public int failcount { get; private set; }
		} // class SaveResult

		public string id { get; set; }
		public List<UiActionEventModel> data { get; set; }

		public SaveResult Save(AConnection oDB, int nBrowserVersionID, string sRemoteIP, string sSessionCookie) {
			var oResult = new SaveResult(id);

			if (data != null)
				foreach (UiActionEventModel oEvt in data)
					oResult.Add(oEvt.eventID, oEvt.Save(oDB, nBrowserVersionID, sRemoteIP, sSessionCookie));

			return oResult;
		} // Save

		public override string ToString() {
			var os = new StringBuilder();

			os.AppendFormat("( id: {0}", id);

			if (data == null)
				os.Append("\n\t-- No data --");
			else {
				foreach (UiActionEventModel oEvt in data)
					os.AppendFormat("\n\t{0}", oEvt);
			} // if

			os.Append("\n)");

			return os.ToString();
		} // ToString

	} // class UiCachePkgModel

} // namespace EZBob.DatabaseLib
