using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using log4net;

namespace EZBob.DatabaseLib {
	#region class UiActionEventModel

	public class UiActionEventModel {
		public string userName { get; set; }
		public string controlName { get; set; }
		public string htmlID { get; set; }
		public string actionName { get; set; }
		public string eventTime { get; set; }
		public string eventID { get; set; }
		public string eventArgs { get; set; }

		#region method Save

		public bool Save(DatabaseDataHelper oHelper, BrowserVersion oBrowserVersion, string sRemoteIP, string sSessionCookie) {
			long nRefNum = 0;

			if (!long.TryParse(eventID, out nRefNum)) {
				ms_oLog.ErrorFormat("Failed to save UI event: cannot parse eventID as long in {0}.", this);
				return false;
			} // if

			UiEvent oEvt = oHelper.UiEventRepository.FindByRefNum(nRefNum);

			if (oEvt != null) {
				ms_oLog.DebugFormat("Not saving UI event: already in DB for {0}.", this);
				return true;
			} // if found

			UiAction oAction = oHelper.UiActionRepository.FindByName(actionName);

			if (oAction == null) {
				ms_oLog.ErrorFormat("Failed to save UI event: action not found in {0}.", this);
				return false;
			} // if action not found

			SecurityUser oUser = oHelper.SecurityUserRepository.FindByName(userName);

			DateTime oTime;

			if (!DateTime.TryParseExact(eventTime, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out oTime)) {
				ms_oLog.ErrorFormat("Failed to save UI event: cannot parse event time in {0}.", this);
				return false;
			} // if failed to parse time

			UiControl oControl = oHelper.UiControlRepository.FindByName(controlName);
			if (oControl == null) {
				oControl = new UiControl { Name = controlName };
				oHelper.UiControlRepository.SaveOrUpdate(oControl);
			} // if no control found

			oEvt = new UiEvent {
				Arguments = eventArgs,
				BrowserVersion = oBrowserVersion,
				HtmlID = htmlID,
				RefNum = nRefNum,
				RemoteIP = sRemoteIP,
				SecurityUser = oUser,
				SessionCookie = sSessionCookie,
				Time = oTime,
				UiAction = oAction,
				UiControl = oControl,
			};

			oHelper.UiEventRepository.SaveOrUpdate(oEvt);

			return true;
		} // Save

		#endregion method Save

		#region method ToString

		public override string ToString() {
			var os = new StringBuilder();

			os.AppendFormat("[ user: {0},", userName);
			os.AppendFormat(" controlName: {0},",  controlName);
			os.AppendFormat(" htmlID: {0},", htmlID);
			os.AppendFormat(" actionName: {0},", actionName);
			os.AppendFormat(" eventTime: {0},", eventTime);
			os.AppendFormat(" eventID: {0},", eventID);
			os.AppendFormat(" eventArgs: {0} ]", eventArgs);

			return os.ToString();
		} // ToString

		#endregion method ToString

		private static readonly ILog ms_oLog = LogManager.GetLogger(typeof(UiActionEventModel));
	} // class UiActionEventModel

	#endregion class UiActionEventModel
} // namespace EZBob.DatabaseLib
