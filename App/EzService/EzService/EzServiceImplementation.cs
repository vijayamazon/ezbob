using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Threading;
using EzBob.Backend.Strategies;
using EzBob.Backend.Strategies.MailStrategies;
using Ezbob.Database;
using Ezbob.Logger;

namespace EzService {
	[ServiceBehavior(
		InstanceContextMode = InstanceContextMode.PerCall,
		IncludeExceptionDetailInFaults = true
	)]
	public class EzServiceImplementation : IEzServiceAdmin, IEzService, IDisposable {
		#region static constructor

		static EzServiceImplementation() {
			ms_oActiveActions = new SortedDictionary<Guid, ActionMetaData>();
			ms_oLockActiveActions = new object();
			ms_oLockTerminateAction = new object();

			ms_oLockNewInstancesAllowed = new object();
			ms_bNewInstancesAllowed = true;
		} // static constructor

		#endregion static constructor

		#region public

		#region constructor

		public EzServiceImplementation(EzServiceInstanceRuntimeData oData) {
			if (!NewInstancesAllowed)
				throw new FaultException("Cannot create EzService instance: new instances are disabled.");

			m_oData = oData;

			m_oLog = ((m_oData != null) && (m_oData.Log != null)) ? m_oData.Log : new SafeLog();

			m_oDB = (m_oData != null) ? m_oData.DB : null;

			m_oLog.Msg("EzService instance created.");
		} // constructor

		#endregion constructor

		#region IEzServiceAdmin exposed methods

		#region method Shutdown

		public ActionMetaData Shutdown() {
			try {
				ActionMetaData amd = NewSync(ActionStatus.Done);

				NewInstancesAllowed = false;

				m_oLog.Msg("Shutdown() method started...");

				lock (ms_oLockActiveActions) {
					foreach (KeyValuePair<Guid, ActionMetaData> kv in ms_oActiveActions)
						kv.Value.UnderlyingThread.Abort();
				} // lock

				if ((m_oData != null) && (m_oData.Host != null)) {
					m_oData.Host.Shutdown();
					amd.Status = ActionStatus.Done;
				}
				else {
					amd.Status = ActionStatus.Finished;
					amd.Comment = "Host data not initialized.";
				} // if

				m_oLog.Msg("Shutdown() method complete with result {0}.", amd);

				SaveActionStatus(amd);

				return amd;
			}
			catch (Exception e) {
				m_oLog.Alert(e, "Exception during Shutdown() method.");
				throw new FaultException(e.Message);
			} // try
		} // Shutdown

		#endregion method Shutdown

		#region method Terminate

		public ActionMetaData Terminate(Guid oActionID) {
			lock (ms_oLockTerminateAction) {
				try {
					ActionMetaData amd = NewSync();

					m_oLog.Msg("Terminate({0}) method started...", oActionID);

					if (oActionID == null) {
						amd.Status = ActionStatus.Finished;
						amd.Comment = "Action ID not specified.";
					}
					else{
						ActionMetaData oCondemned = null;

						lock (ms_oLockActiveActions) {
							if (ms_oActiveActions.ContainsKey(oActionID))
								oCondemned = ms_oActiveActions[oActionID];
						} // lock

						if (oCondemned == null) {
							amd.Status = ActionStatus.Finished;
							amd.Comment = "Requested action not found.";
						}
						else {
							oCondemned.UnderlyingThread.Abort();

							oCondemned.Status = ActionStatus.Terminated;
							SaveActionStatus(oCondemned);

							amd.Status = ActionStatus.Done;
						} // if (not) found action
					} // if (not) specified id of action to terminate

					m_oLog.Msg("Terminate({0}) method complete with result {1}.", oActionID, amd);

					SaveActionStatus(amd);
					return amd;
				}
				catch (Exception e) {
					m_oLog.Alert(e, "Exception during Terminate(Guid) method.");
					throw new FaultException(e.Message);
				} // try
			} // lock
		} // Terminate

		#endregion method Terminate

		#region method Nop

		public ActionMetaData Nop(int nLengthInSeconds) {
			try {
				m_oLog.Msg("Nop({0}) method started...", nLengthInSeconds);

				ActionMetaData amd = NewAsync();

				if (nLengthInSeconds < 1)
					throw new Exception("Nop length is less than 1 second.");

				m_oLog.Msg("Nop({0}) method: creating a sleeper...", nLengthInSeconds);

				amd.UnderlyingThread = new Thread(() => {
					for (int i = 1; i <= nLengthInSeconds; i++) {
						m_oLog.Msg("Nop({0}) method asleeper: {1}...", nLengthInSeconds, i);
						Thread.Sleep(1000);
					} // for

					amd.Status = ActionStatus.Done;

					m_oLog.Msg("Nop({0}) method asleeper: completed: {1}.", nLengthInSeconds, amd);

					SaveActionStatus(amd);
				});

				m_oLog.Msg("Nop({0}) method: starting asleeper...", nLengthInSeconds);

				amd.UnderlyingThread.Start();

				m_oLog.Msg("Nop({0}) method: asleeper started: {1}.", nLengthInSeconds, amd);

				m_oLog.Msg("Nop({0}) method complete.", nLengthInSeconds);

				return amd;
			}
			catch (Exception e) {
				m_oLog.Alert(e, "Exception during Nop() method.");
				throw new FaultException(e.Message);
			} // try
		} // Nop

		#endregion method Nop

		#region method ListActiveActions

		public StringListActionResult ListActiveActions() {
			try {
				ActionMetaData amd = NewSync();

				m_oLog.Msg("ListActiveActions() method started...");

				var oResult = new List<string>();

				lock (ms_oLockActiveActions) {
					foreach (KeyValuePair<Guid, ActionMetaData> kv in ms_oActiveActions)
						oResult.Add(kv.Value + " - thread state: " + kv.Value.UnderlyingThread.ThreadState);
				} // lock

				amd.Status = ActionStatus.Done;
				SaveActionStatus(amd);
				m_oLog.Msg("ListActiveActions() method complete with result {0}.", amd);

				return new StringListActionResult { MetaData = amd, Records = oResult };
			}
			catch (Exception e) {
				m_oLog.Alert(e, "Exception during ListActiveActions() method.");
				throw new FaultException(e.Message);
			} // try
		} // ListActiveActions

		#endregion method ListActiveActions

		#region method WriteToLog

		public ActionMetaData WriteToLog(string sSeverity, string sMsg) {
			try {
				ActionMetaData amd = NewSync(ActionStatus.Done);

				m_oLog.Msg("WriteToLog() method started...");

				Severity nSeverity = Severity.Info;

				Severity.TryParse(sSeverity, true, out nSeverity);

				m_oLog.Debug("Requested severity: {0}, actual severity: {1}", sSeverity, nSeverity);

				m_oLog.Say(nSeverity, sMsg);

				m_oLog.Msg("WriteToLog() method complete with result {0}.", amd);

				SaveActionStatus(amd);

				return amd;
			}
			catch (Exception e) {
				m_oLog.Alert(e, "Exception during WriteToLog() method.");
				throw new FaultException(e.Message);
			} // try
		} // WriteToLog

		#endregion method WriteToLog

		#endregion IEzServiceAdmin exposed methods

		#region IEzService exposed methods

		public ActionMetaData GreetingMailStrategy(int nCustomerID, string sConfirmationEmail) {
			return Execute(typeof(Greeting), nCustomerID, sConfirmationEmail);
		} // GreetingMailStrategy

		public ActionMetaData CustomerMarketplaceAdded(int nCustomerID, int nMarketplaceID) {
			return Execute(typeof (UpdateMarketplace), nCustomerID, nMarketplaceID);
		} // CustomerMarketplaceAdded

		public ActionMetaData ApprovedUser(int customerId, decimal loanAmount) {
			return Execute(typeof (ApprovedUser), customerId, loanAmount);
		} // ApprovedUser

		public ActionMetaData CashTransferred(int customerId, decimal amount) {
			return Execute(typeof (CashTransferred), customerId, amount);
		} // CashTransferred

		public ActionMetaData EmailUnderReview(int customerId) {
			return Execute(typeof (EmailUnderReview), customerId);
		} // EmailUnderReview

		public ActionMetaData Escalated(int customerId) {
			return Execute(typeof (Escalated), customerId);
		} // Escalated

		public ActionMetaData GetCashFailed(int customerId) {
			return Execute(typeof (GetCashFailed), customerId);
		} // GetCashFailed

		public ActionMetaData LoanFullyPaid(int customerId, string loanRefNum) {
			return Execute(typeof(LoanFullyPaid), customerId, loanRefNum);
		} // LoanFullyPaid

		public ActionMetaData MoreAmLandBwaInformation(int customerId) {
			return Execute(typeof (MoreAMLandBWAInformation), customerId);
		} // MoreAmLandBwaInformation

		public ActionMetaData MoreAmlInformation(int customerId) {
			return Execute(typeof (MoreAMLInformation), customerId);
		} // MoreAmlInformation

		public ActionMetaData MoreBwaInformation(int customerId) {
			return Execute(typeof (MoreBWAInformation), customerId);
		} // MoreBwaInformation

		public ActionMetaData PasswordChanged(int customerId, string password) {
			return Execute(typeof (PasswordChanged), customerId, password);
		} // PasswordChanged

		public ActionMetaData PasswordRestored(int customerId, string password) {
			return Execute(typeof (PasswordRestored), customerId, password);
		} // PasswordRestored

		public ActionMetaData PayEarly(int customerId, decimal amount, string loanRefNum) {
			return Execute(typeof (PayEarly), customerId, amount, loanRefNum);
		} // PayEarly

		public ActionMetaData PayPointAddedByUnderwriter(int customerId, string cardno, string underwriterName, int underwriterId) {
			return Execute(typeof (PayPointAddedByUnderwriter), customerId, cardno, underwriterName, underwriterId);
		} // PayPointAddedByUnderwriter

		public ActionMetaData PayPointNameValidationFailed(int customerId, string cardHolderName) {
			return Execute(typeof (PayPointNameValidationFailed), customerId, cardHolderName);
		} // PayPointNameValidationFailed

		public ActionMetaData RejectUser(int customerId) {
			return Execute(typeof (RejectUser), customerId);
		} // RejectUser

		public ActionMetaData EmailRolloverAdded(int customerId, decimal amount) {
			return Execute(typeof(EmailRolloverAdded), customerId, amount);
		} // EmailRolloverAdded

		public ActionMetaData RenewEbayToken(int customerId, string marketplaceName, string eBayAddress) {
			return Execute(typeof (RenewEbayToken), customerId, marketplaceName, eBayAddress);
		} // RenewEbayToken

		public ActionMetaData RequestCashWithoutTakenLoan(int customerId) {
			return Execute(typeof (RequestCashWithoutTakenLoan), customerId);
		} // RequestCashWithoutTakenLoan

		public ActionMetaData SendEmailVerification(int customerId, string address) {
			return Execute(typeof (SendEmailVerification), customerId, address);
		} // SendEmailVerification

		public ActionMetaData ThreeInvalidAttempts(int customerId, string password) {
			return Execute(typeof (ThreeInvalidAttempts), customerId, password);
		} // ThreeInvalidAttempts

		public ActionMetaData TransferCashFailed(int customerId) {
			return Execute(typeof (TransferCashFailed), customerId);
		} // TransferCashFailed

		public ActionMetaData CaisGenerate(int underwriterId) {
			return Execute(typeof (CaisGenerate), underwriterId);
		} // CaisGenerate

		public ActionMetaData CaisUpdate(int caisId) {
			return Execute(typeof (CaisUpdate), caisId);
		} // CaisUpdate

		public ActionMetaData FirstOfMonthStatusNotifier() {
			return Execute(typeof (FirstOfMonthStatusNotifier));
		} // FirstOfMonthStatusNotifier

		public ActionMetaData FraudChecker(int customerId) {
			return Execute(typeof (FraudChecker), customerId);
		} // FraudChecker

		public ActionMetaData LateBy14Days() {
			return Execute(typeof (LateBy14Days));
		} // LateBy14Days

		public ActionMetaData PayPointCharger() {
			return Execute(typeof (PayPointCharger));
		} // PayPointCharger

		public ActionMetaData SetLateLoanStatus() {
			return Execute(typeof (SetLateLoanStatus));
		} // SetLateLoanStatus

		public ActionMetaData UpdateMarketplace(int customerId, int marketplaceId) {
			return Execute(typeof (UpdateMarketplace), customerId, marketplaceId);
		} // UpdateMarketplace

		public ActionMetaData UpdateAllMarketplaces(int customerId) {
			return Execute(typeof (UpdateMarketplaces), customerId);
		} // UpdateAllMarketplaces

		public ActionMetaData UpdateTransactionStatus() {
			return Execute(typeof (UpdateTransactionStatus));
		} // UpdateTransactionStatus

		public ActionMetaData XDaysDue() {
			return Execute(typeof (XDaysDue));
		} // XDaysDue

		#endregion IEzService exposed methods

		#region method IDisposable.Dispose

		public void Dispose() {
			// TODO: empty so far
			m_oLog.Msg("EzService instance disposed.");
		} // Dispose

		#endregion method IDisposable.Dispose

		#endregion public

		#region private

		#region property NewInstancesAllowed

		private bool NewInstancesAllowed {
			get {
				bool b;

				lock (ms_oLockNewInstancesAllowed) {
					b = ms_bNewInstancesAllowed;
				} // lock

				return b;
			} // get
			set {
				lock (ms_oLockNewInstancesAllowed) {
					ms_bNewInstancesAllowed = value;
				} // lock
			} // set
		} // NewInstancesAllowed

		#endregion property NewInstancesAllowed

		#region method SaveActionStatus

		private void SaveActionStatus(ActionMetaData amd) {
			m_oLog.Debug("EzService: saving action status to DB: {0}.", amd);

			if (amd.IsComplete() == TriState.Yes) {
				lock (ms_oLockActiveActions) {
					if (ms_oActiveActions.ContainsKey(amd.ActionID))
						ms_oActiveActions.Remove(amd.ActionID);
				} // lock
			} // if

			// TODO: save to DB
		} // SaveActionStatus

		#endregion method SaveActionStatus

		#region method Execute

		private ActionMetaData Execute(Type oStrategyType, params object[] args) {
			ActionMetaData amd = null;

			try {
				m_oLog.Debug("Executing " + oStrategyType + " started...");

				amd = NewAsync();

				var oParams = new List<object>(args) { m_oDB, m_oLog };

				ConstructorInfo oCreator = oStrategyType.GetConstructors().FirstOrDefault(ci => ci.GetParameters().Length == oParams.Count);

				if (oCreator == null)
					throw new Exception("Failed to find a constructor for " + oStrategyType + " with " + oParams.Count + " arguments.");

				m_oLog.Debug(oStrategyType + " constructor found, invoking...");

				amd.UnderlyingThread = new Thread(() =>
				{
					((AStrategy)oCreator.Invoke(oParams.ToArray())).Execute();

					amd.Status = ActionStatus.Done;

					m_oLog.Debug("Executing " + oStrategyType + " complete.");

					SaveActionStatus(amd);
				});
				amd.UnderlyingThread.Start();

				m_oLog.Debug("Executing " + oStrategyType + " started on another thread.");

				return amd;
			}
			catch (Exception e) {
				if (amd != null) {
					amd.Status = ActionStatus.Failed;
					SaveActionStatus(amd);
				} // if

				m_oLog.Alert(e, "Exception during executing " + oStrategyType + " strategy.");
				throw new FaultException(e.Message);
			} // try
		} // Execute

		#endregion method Execute

		#region static methods

		#region method NewAsync

		private ActionMetaData NewAsync(ActionStatus status = ActionStatus.InProgress, string comment = null) {
			return CreateActionMetaData(false, status, comment);
		} // NewAsync

		#endregion method NewAsync

		#region method NewSync

		private ActionMetaData NewSync(ActionStatus status = ActionStatus.InProgress, string comment = null) {
			return CreateActionMetaData(true, status, comment);
		} // NewSync

		#endregion method NewSync

		#region method CreateActionMetaData

		private ActionMetaData CreateActionMetaData(bool bIsSynchronous, ActionStatus status = ActionStatus.InProgress, string comment = null) {
			var amd = bIsSynchronous ? ActionMetaData.NewSync(status, comment) : ActionMetaData.NewAsync(status, comment);

			lock (ms_oLockActiveActions) {
				ms_oActiveActions[amd.ActionID] = amd;
			} // lock

			SaveActionStatus(amd);

			return amd;
		} // CreateActionMetaData

		#endregion method CreateActionMetaData

		#endregion static methods

		#region properties

		private readonly EzServiceInstanceRuntimeData m_oData;
		private readonly ASafeLog m_oLog;
		private readonly AConnection m_oDB;

		private static readonly SortedDictionary<Guid, ActionMetaData> ms_oActiveActions;
		private static readonly object ms_oLockActiveActions;
		private static readonly object ms_oLockTerminateAction;

		private static bool ms_bNewInstancesAllowed;
		private static readonly object ms_oLockNewInstancesAllowed;

		#endregion properties

		#endregion private
	} // class EzServiceImplementation
} // namespace EzService
