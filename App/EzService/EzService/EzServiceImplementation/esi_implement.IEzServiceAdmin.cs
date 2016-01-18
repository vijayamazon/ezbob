namespace EzService.EzServiceImplementation {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.ServiceModel;
	using System.Threading;
	using Ezbob.Backend.Strategies;
	using Ezbob.Backend.Strategies.Admin;
	using Ezbob.Logger;
	using Ezbob.Utils.Exceptions;

	partial class EzServiceImplementation {

		public ActionMetaData Shutdown() {
			ActionMetaData amd = null;

			try {
				amd = NewSync("Admin.Shutdown", ActionStatus.Done);

				NewInstancesAllowed = false;

				Log.Msg("Shutdown() method started...");

				lock (lockActiveActions) {
					foreach (KeyValuePair<Guid, ActionMetaData> kv in activeActions)
						kv.Value.UnderlyingThread.Abort();
				} // lock

				ActionStatus nNewStatus;

				if ((this.data != null) && (this.data.Host != null)) {
					this.data.Host.Shutdown();
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

				if (!(e is AException))
					Log.Alert(e, "Exception during Shutdown() method.");

				throw new FaultException(e.Message);
			} // try
		} // Shutdown

		public ActionMetaData Terminate(Guid oActionID) {
			lock (lockTerminateAction) {
				try {
					ActionMetaData amd = NewSync("Admin.Terminate", comment: "condemned: " + oActionID);

					Log.Msg("Terminate({0}) method started...", oActionID);

					ActionStatus nNewStatus;

					ActionMetaData oCondemned = null;

					lock (lockActiveActions) {
						if (activeActions.ContainsKey(oActionID))
							oCondemned = activeActions[oActionID];
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

					SaveActionStatus(amd, nNewStatus);

					Log.Msg("Terminate({0}) method complete with result {1}.", oActionID, amd);

					return amd;
				}
				catch (Exception e) {
					if (!(e is AException))
						Log.Alert(e, "Exception during Terminate(Guid) method.");

					throw new FaultException(e.Message);
				} // try
			} // lock
		} // Terminate

		public ActionMetaData Nop(int nLengthInSeconds, string sMsg) {
			return Execute<Nop>(null, null, nLengthInSeconds, sMsg);
		} // Nop

		public ActionMetaData Noop() {
			return Execute<Noop>(null, null);
		} // Noop

		public ActionMetaData StressTestAction(int nLengthInSeconds, string sMsg) {
			try {
				int nActiveCount = 0;

				lock (lockActiveActions) {
					nActiveCount = activeActions.Count;
				} // lock

				sMsg = (sMsg ?? string.Empty).Trim();

				Log.Msg("StressTestAction({0}, {1}) method started, {2} currently active actions...", nLengthInSeconds, sMsg, nActiveCount);

				ActionMetaData amd = NewAsync("Admin.StressTestAction", comment: nLengthInSeconds + " seconds, " + sMsg);

				if (nLengthInSeconds < 1)
					throw new Warning(Log, "StressTestAction length is less than 1 second.");

				Log.Msg("StressTestAction({0}, {1}) method: creating a sleeper...", nLengthInSeconds, sMsg);

				amd.UnderlyingThread = new Thread(() => {
					for (int i = 1; i <= nLengthInSeconds; i++) {
						Log.Msg("StressTestAction({0}, {2}) method asleeper: {1}...", nLengthInSeconds, i, sMsg);
						Thread.Sleep(1000);
					} // for

					SaveActionStatus(amd, ActionStatus.Done);

					Log.Msg("StressTestAction({0}, {2}) method asleeper: completed: {1}.", nLengthInSeconds, amd, sMsg);
				});

				Log.Msg("StressTestAction({0}, {1}) method: starting asleeper...", nLengthInSeconds, sMsg);

				SaveActionStatus(amd, ActionStatus.Launched);

				amd.UnderlyingThread.Start();

				Log.Msg("StressTestAction({0}, {2}) method: asleeper started: {1}.", nLengthInSeconds, amd, sMsg);

				Log.Msg("StressTestAction({0}, {1}) method complete.", nLengthInSeconds, sMsg);

				return amd;
			}
			catch (Exception e) {
				if (!(e is AException))
					Log.Alert(e, "Exception during StressTestAction() method.");

				throw new FaultException(e.Message);
			} // try
		} // StressTestAction

		public ActionMetaData StressTestSync(int nLengthInSeconds, string sMsg) {
			try {
				int nActiveCount = 0;

				lock (lockActiveActions) {
					nActiveCount = activeActions.Count;
				} // lock

				sMsg = (sMsg ?? string.Empty).Trim();

				Log.Msg("StressTestSync({0}, {1}) method started, {2} currently active actions...", nLengthInSeconds, sMsg, nActiveCount);

				ActionMetaData amd = NewSync("Admin.StressTestSync", comment: nLengthInSeconds + " seconds, " + sMsg);

				if (nLengthInSeconds < 1)
					throw new Warning(Log, "StressTestSync length is less than 1 second.");

				Log.Msg("StressTestSync({0}, {2}) method: asleeper started: {1}.", nLengthInSeconds, amd, sMsg);

				for (int i = 1; i <= nLengthInSeconds; i++) {
					Log.Msg("StressTestSync({0}, {2}) method asleeper: {1}...", nLengthInSeconds, i, sMsg);
					Thread.Sleep(1000);
				} // for

				Log.Msg("StressTestSync({0}, {2}) method asleeper: completed: {1}.", nLengthInSeconds, amd, sMsg);

				Log.Msg("StressTestSync({0}, {1}) method complete.", nLengthInSeconds, sMsg);

				SaveActionStatus(amd, ActionStatus.Done);

				return amd;
			}
			catch (Exception e) {
				if (!(e is AException))
					Log.Alert(e, "Exception during StressTestSync() method.");

				throw new FaultException(e.Message);
			} // try
		} // StressTestSync

		public StringListActionResult ListActiveActions() {
			try {
				ActionMetaData amd = NewSync("Admin.ListActiveActions");

				Log.Msg("ListActiveActions() method started...");

				var oResult = new List<string>();

				lock (lockActiveActions) {
					oResult.AddRange(activeActions.Select(
						kv => {
							AStrategy stra = kv.Value.Strategy;

							return string.Format(
								"{0} {2}- thread state: {1}",
								kv.Value,
								kv.Value.UnderlyingThread.ThreadState,
								stra == null ? string.Empty : "(" + stra.Context + ") "
							);
						}
					));
				} // lock

				SaveActionStatus(amd, ActionStatus.Done);

				Log.Msg("ListActiveActions() method complete with result {0}.", amd);

				return new StringListActionResult { MetaData = amd, Records = oResult };
			}
			catch (Exception e) {
				if (!(e is AException))
					Log.Alert(e, "Exception during ListActiveActions() method.");

				throw new FaultException(e.Message);
			} // try
		} // ListActiveActions

		public ActionMetaData WriteToLog(string sSeverity, string sMsg) {
			ActionMetaData amd = null;

			try {
				amd = NewSync("Admin.WriteToLog", comment: string.Format("{0}: {1}", sSeverity, sMsg));

				Log.Msg("WriteToLog() method started...");

				Severity nSeverity;

				if (!Enum.TryParse(sSeverity, true, out nSeverity))
					nSeverity = Severity.Info;

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

				if (!(e is AException))
					Log.Alert(e, "Exception during WriteToLog() method.");

				throw new FaultException(e.Message);
			} // try
		} // WriteToLog

	} // class EzServiceImplementation
} // namespace EzService
