namespace EzService.EzServiceImplementation {
	using System;
	using System.Collections.Generic;
	using System.ServiceModel;
	using System.Threading;
	using Ezbob.Logger;

	partial class EzServiceImplementation {
		#region method Shutdown

		public ActionMetaData Shutdown() {
			ActionMetaData amd = null;

			try {
				amd = NewSync("Admin.Shutdown", ActionStatus.Done);

				NewInstancesAllowed = false;

				Log.Msg("Shutdown() method started...");

				lock (ms_oLockActiveActions) {
					foreach (KeyValuePair<Guid, ActionMetaData> kv in ms_oActiveActions)
						kv.Value.UnderlyingThread.Abort();
				} // lock

				ActionStatus nNewStatus;

				if ((m_oData != null) && (m_oData.Host != null)) {
					m_oData.Host.Shutdown();
					nNewStatus = ActionStatus.Done;
				}
				else {
					amd.Comment = "Host data not initialized.";
					nNewStatus = ActionStatus.Finished;
				} // if

				Log.Msg("Shutdown() method complete with result {0}.", amd);

				SaveActionStatus(amd, nNewStatus);

				return amd;
			}
			catch (Exception e) {
				if (amd != null) {
					amd.Comment = e.Message;
					SaveActionStatus(amd, ActionStatus.Failed);
				} // if

				Log.Alert(e, "Exception during Shutdown() method.");
				throw new FaultException(e.Message);
			} // try
		} // Shutdown

		#endregion method Shutdown

		#region method Terminate

		public ActionMetaData Terminate(Guid oActionID) {
			lock (ms_oLockTerminateAction) {
				try {
					ActionMetaData amd = NewSync("Admin.Terminate", comment: "condemned: " + oActionID);

					Log.Msg("Terminate({0}) method started...", oActionID);

					ActionStatus nNewStatus;

					if (ReferenceEquals(oActionID, null)) {
						amd.Comment = "Action ID not specified.";
						nNewStatus = ActionStatus.Finished;
					}
					else {
						ActionMetaData oCondemned = null;

						lock (ms_oLockActiveActions) {
							if (ms_oActiveActions.ContainsKey(oActionID))
								oCondemned = ms_oActiveActions[oActionID];
						} // lock

						if (oCondemned == null) {
							amd.Comment = "Requested action not found.";
							nNewStatus = ActionStatus.Finished;
						}
						else {
							oCondemned.UnderlyingThread.Abort();

							SaveActionStatus(oCondemned, ActionStatus.Terminated);

							nNewStatus = ActionStatus.Done;
						} // if (not) found action
					} // if (not) specified id of action to terminate

					SaveActionStatus(amd, nNewStatus);

					Log.Msg("Terminate({0}) method complete with result {1}.", oActionID, amd);

					return amd;
				}
				catch (Exception e) {
					Log.Alert(e, "Exception during Terminate(Guid) method.");
					throw new FaultException(e.Message);
				} // try
			} // lock
		} // Terminate

		#endregion method Terminate

		#region method Nop

		public ActionMetaData Nop(int nLengthInSeconds) {
			try {
				Log.Msg("Nop({0}) method started...", nLengthInSeconds);

				ActionMetaData amd = NewAsync("Admin.Nop", comment: nLengthInSeconds + " seconds");

				if (nLengthInSeconds < 1)
					throw new Exception("Nop length is less than 1 second.");

				Log.Msg("Nop({0}) method: creating a sleeper...", nLengthInSeconds);

				amd.UnderlyingThread = new Thread(() => {
					for (int i = 1; i <= nLengthInSeconds; i++) {
						Log.Msg("Nop({0}) method asleeper: {1}...", nLengthInSeconds, i);
						Thread.Sleep(1000);
					} // for

					SaveActionStatus(amd, ActionStatus.Done);

					Log.Msg("Nop({0}) method asleeper: completed: {1}.", nLengthInSeconds, amd);
				});

				Log.Msg("Nop({0}) method: starting asleeper...", nLengthInSeconds);

				SaveActionStatus(amd, ActionStatus.Launched);

				amd.UnderlyingThread.Start();

				Log.Msg("Nop({0}) method: asleeper started: {1}.", nLengthInSeconds, amd);

				Log.Msg("Nop({0}) method complete.", nLengthInSeconds);

				return amd;
			}
			catch (Exception e) {
				Log.Alert(e, "Exception during Nop() method.");
				throw new FaultException(e.Message);
			} // try
		} // Nop

		#endregion method Nop

		#region method ListActiveActions

		public StringListActionResult ListActiveActions() {
			try {
				ActionMetaData amd = NewSync("Admin.ListActiveActions");

				Log.Msg("ListActiveActions() method started...");

				var oResult = new List<string>();

				lock (ms_oLockActiveActions) {
					foreach (KeyValuePair<Guid, ActionMetaData> kv in ms_oActiveActions)
						oResult.Add(kv.Value + " - thread state: " + kv.Value.UnderlyingThread.ThreadState);
				} // lock

				SaveActionStatus(amd, ActionStatus.Done);

				Log.Msg("ListActiveActions() method complete with result {0}.", amd);

				return new StringListActionResult { MetaData = amd, Records = oResult };
			}
			catch (Exception e) {
				Log.Alert(e, "Exception during ListActiveActions() method.");
				throw new FaultException(e.Message);
			} // try
		} // ListActiveActions

		#endregion method ListActiveActions

		#region method WriteToLog

		public ActionMetaData WriteToLog(string sSeverity, string sMsg) {
			ActionMetaData amd = null;

			try {
				amd = NewSync("Admin.WriteToLog", comment: string.Format("{0}: {1}", sSeverity, sMsg));

				Log.Msg("WriteToLog() method started...");

				Severity nSeverity = Severity.Info;

				Severity.TryParse(sSeverity, true, out nSeverity);

				Log.Debug("Requested severity: {0}, actual severity: {1}", sSeverity, nSeverity);

				Log.Say(nSeverity, sMsg);

				Log.Msg("WriteToLog() method complete with result {0}.", amd);

				SaveActionStatus(amd, ActionStatus.Done);

				return amd;
			}
			catch (Exception e) {
				if (amd != null) {
					amd.Comment = e.Message;
					SaveActionStatus(amd, ActionStatus.Done);
				} // if

				Log.Alert(e, "Exception during WriteToLog() method.");
				throw new FaultException(e.Message);
			} // try
		} // WriteToLog

		#endregion method WriteToLog
	} // class EzServiceImplementation
} // namespace EzService
