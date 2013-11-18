﻿using System.Collections.Generic;
using System.Text;

namespace EZBob.DatabaseLib {
	#region class UiCachePkgModel

	public class UiCachePkgModel {
		#region class SaveResult

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

		#endregion class SaveResult

		public string id { get; set; }
		public List<UiActionEventModel> data { get; set; }

		#region method Save

		public SaveResult Save(DatabaseDataHelper oHelper, BrowserVersion oBrowserVersion, string sRemoteIP, string sSessionCookie) {
			var oResult = new SaveResult(id);

			if (data != null)
				foreach (UiActionEventModel oEvt in data)
					oResult.Add(oEvt.eventID, oEvt.Save(oHelper, oBrowserVersion, sRemoteIP, sSessionCookie));

			return oResult;
		} // Save

		#endregion method Save

		#region method ToString

		public override string ToString() {
			var os = new StringBuilder();

			os.AppendFormat("( id: {0}", id);

			if (data == null)
				os.Append("\n\t-- No data --");
			else {
				foreach (var oEvt in data)
					os.AppendFormat("\n\t{0}", oEvt.ToString());
			} // if

			os.Append("\n)");

			return os.ToString();
		} // ToString

		#endregion method ToString
	} // class UiCachePkgModel


	#endregion class UiCachePkgModel
} // namespace EZBob.DatabaseLib
