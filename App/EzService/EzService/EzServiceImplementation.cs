namespace EzService {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.ServiceModel;
	using System.Threading;
	using EzBob.Backend.Strategies;
	using EzBob.Backend.Strategies.MailStrategies;
	using EzBob.Backend.Strategies.QuickOffer;
	using Ezbob.Database;
	using Ezbob.Logger;
	using FraudChecker;

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

			Log.Msg("EzService instance created.");
		} // constructor

		#endregion constructor

		#region IEzServiceAdmin exposed methods

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

		#endregion IEzServiceAdmin exposed methods

		#region IEzService exposed methods

		public ActionMetaData GreetingMailStrategy(int nCustomerID, string sConfirmationEmail) {
			return Execute(nCustomerID, null, typeof(Greeting), nCustomerID, sConfirmationEmail);
		} // GreetingMailStrategy

		public ActionMetaData ApprovedUser(int userId, int customerId, decimal loanAmount) {
			return Execute(customerId, userId, typeof(ApprovedUser), customerId, loanAmount);
		} // ApprovedUser

		public ActionMetaData CashTransferred(int customerId, decimal amount) {
			return Execute(customerId, null, typeof(CashTransferred), customerId, amount);
		} // CashTransferred

		public ActionMetaData EmailUnderReview(int customerId) {
			return Execute(customerId, null, typeof(EmailUnderReview), customerId);
		} // EmailUnderReview

		public ActionMetaData Escalated(int customerId) {
			return Execute(customerId, customerId, typeof(Escalated), customerId);
		} // Escalated

		public ActionMetaData GetCashFailed(int customerId) {
			return Execute(customerId, null, typeof(GetCashFailed), customerId);
		} // GetCashFailed

		public ActionMetaData LoanFullyPaid(int customerId, string loanRefNum) {
			return Execute(customerId, null, typeof(LoanFullyPaid), customerId, loanRefNum);
		} // LoanFullyPaid

		public ActionMetaData MoreAmlAndBwaInformation(int userId, int customerId) {
			return Execute(customerId, userId, typeof(MoreAmlAndBwaInformation), customerId);
		} // MoreAmlAndBwaInformation

		public ActionMetaData MoreAmlInformation(int userId, int customerId) {
			return Execute(customerId, userId, typeof(MoreAmlInformation), customerId);
		} // MoreAmlInformation

		public ActionMetaData MoreBwaInformation(int userId, int customerId) {
			return Execute(customerId, userId, typeof(MoreBwaInformation), customerId);
		} // MoreBwaInformation

		public ActionMetaData PasswordChanged(int customerId, string password) {
			return Execute(customerId, null, typeof(PasswordChanged), customerId, password);
		} // PasswordChanged

		public ActionMetaData PasswordRestored(int customerId, string password) {
			return Execute(customerId, null, typeof(PasswordRestored), customerId, password);
		} // PasswordRestored

		public ActionMetaData PayEarly(int customerId, decimal amount, string loanRefNum) {
			return Execute(customerId, customerId, typeof(PayEarly), customerId, amount, loanRefNum);
		} // PayEarly

		public ActionMetaData PayPointAddedByUnderwriter(int customerId, string cardno, string underwriterName, int underwriterId) {
			return Execute(customerId, underwriterId, typeof(PayPointAddedByUnderwriter), customerId, cardno, underwriterName, underwriterId);
		} // PayPointAddedByUnderwriter

		public ActionMetaData PayPointNameValidationFailed(int userId, int customerId, string cardHolderName) {
			return Execute(customerId, userId, typeof(PayPointNameValidationFailed), customerId, cardHolderName);
		} // PayPointNameValidationFailed

		public ActionMetaData RejectUser(int userId, int customerId) {
			return Execute(customerId, userId, typeof(RejectUser), customerId);
		} // RejectUser

		public ActionMetaData EmailRolloverAdded(int customerId, decimal amount) {
			return Execute(customerId, customerId, typeof(EmailRolloverAdded), customerId, amount);
		} // EmailRolloverAdded

		public ActionMetaData RenewEbayToken(int customerId, string marketplaceName, string eBayAddress) {
			return Execute(customerId, customerId, typeof(RenewEbayToken), customerId, marketplaceName, eBayAddress);
		} // RenewEbayToken

		public ActionMetaData RequestCashWithoutTakenLoan(int customerId) {
			return Execute(customerId, null, typeof(RequestCashWithoutTakenLoan), customerId);
		} // RequestCashWithoutTakenLoan

		public ActionMetaData SendEmailVerification(int customerId, string email, string address) {
			return Execute(customerId, customerId, typeof(SendEmailVerification), customerId, email, address);
		} // SendEmailVerification

		public ActionMetaData ThreeInvalidAttempts(int customerId, string password) {
			return Execute(customerId, null, typeof(ThreeInvalidAttempts), customerId, password);
		} // ThreeInvalidAttempts

		public ActionMetaData TransferCashFailed(int customerId) {
			return Execute(customerId, null, typeof(TransferCashFailed), customerId);
		} // TransferCashFailed

		public ActionMetaData CaisGenerate(int underwriterId) {
			return Execute(null, underwriterId, typeof(CaisGenerate), underwriterId);
		} // CaisGenerate

		public ActionMetaData CaisUpdate(int userId, int caisId) {
			return Execute(null, userId, typeof(CaisUpdate), caisId);
		} // CaisUpdate

		public ActionMetaData FirstOfMonthStatusNotifier() {
			return Execute(null, null, typeof(FirstOfMonthStatusNotifier));
		} // FirstOfMonthStatusNotifier

		public ActionMetaData FraudChecker(int customerId, FraudMode mode) {
			return Execute(customerId, null, typeof (FraudChecker), customerId, mode);
		} // FraudChecker

		public ActionMetaData LateBy14Days() {
			return Execute(null, null, typeof(LateBy14Days));
		} // LateBy14Days

		public ActionMetaData PayPointCharger() {
			return Execute(null, null, typeof(PayPointCharger));
		} // PayPointCharger

		public ActionMetaData SetLateLoanStatus() {
			return Execute(null, null, typeof(SetLateLoanStatus));
		} // SetLateLoanStatus

		public ActionMetaData UpdateMarketplace(int customerId, int marketplaceId) {
			return Execute(customerId, null, typeof(UpdateMarketplace), customerId, marketplaceId);
		} // UpdateMarketplace

		public ActionMetaData UpdateAllMarketplaces(int customerId) {
			return Execute(customerId, null, typeof(UpdateMarketplaces), customerId);
		} // UpdateAllMarketplaces

		public ActionMetaData UpdateTransactionStatus() {
			return Execute(null, null, typeof(UpdateTransactionStatus));
		} // UpdateTransactionStatus

		public ActionMetaData XDaysDue() {
			return Execute(null, null, typeof(XDaysDue));
		} // XDaysDue

		public ActionMetaData CheckExperianCompany(int customerId) {
			return Execute(customerId, null, typeof(ExperianCompanyCheck), customerId);
		} // CheckExperianCompany

		public ActionMetaData CheckExperianConsumer(int customerId, int directorId) {
			return Execute(customerId, null, typeof(ExperianConsumerCheck), customerId, directorId);
		} // CheckExperianConsumer

		public ActionMetaData CheckAml(int customerId) {
			return Execute(customerId, null, typeof(AmlChecker), customerId);
		} // CheckAml

		public ActionMetaData CheckAmlCustom(
			int customerId, string idhubHouseNumber, string idhubHouseName, string idhubStreet,
			string idhubDistrict, string idhubTown, string idhubCounty, string idhubPostCode
		) {
			return Execute(customerId, null, typeof(AmlChecker), customerId, idhubHouseNumber, idhubHouseName, idhubStreet, idhubDistrict, idhubTown, idhubCounty, idhubPostCode);
		} // CheckAmlCustom

		public ActionMetaData CheckBwa(int customerId) {
			return Execute(customerId, null, typeof(BwaChecker), customerId);
		} // CheckBwa

		public ActionMetaData CheckBwaCustom(
			int customerId, string idhubHouseNumber, string idhubHouseName, string idhubStreet,
			string idhubDistrict, string idhubTown, string idhubCounty, string idhubPostCode,
			string idhubBranchCode, string idhubAccountNumber
		) {
			return Execute(
				customerId, null, typeof(BwaChecker), customerId, idhubHouseNumber, idhubHouseName, idhubStreet,
				idhubDistrict, idhubTown, idhubCounty, idhubPostCode,
				idhubBranchCode, idhubAccountNumber
			);
		} // CheckBwaCustom

		public ActionMetaData MainStrategy1(int underwriterId, int customerId, NewCreditLineOption newCreditLine, int avoidAutoDescison) {
			return Execute(customerId, underwriterId, typeof(MainStrategy), customerId, newCreditLine, avoidAutoDescison);
		} // MainStrategy1

		public ActionMetaData MainStrategy2(int underwriterId, int customerId, NewCreditLineOption newCreditLine, int avoidAutoDescison, bool isUnderwriterForced) {
			return Execute(customerId, underwriterId, typeof(MainStrategy), customerId, newCreditLine, avoidAutoDescison, isUnderwriterForced);
		} // MainStrategy2

		public ActionMetaData MainStrategy3(int underwriterId, int customerId, int checkType, string houseNumber, string houseName, string street, string district, string town, string county, string postcode, string bankAccount, string sortCode, int avoidAutoDescison) {
			return Execute(customerId, underwriterId, typeof(MainStrategy), customerId, checkType, houseNumber, houseName, street, district, town, county, postcode, bankAccount, sortCode, avoidAutoDescison);
		} // MainStrategy3

		public ActionMetaData MainStrategySync1(int underwriterId, int customerId, NewCreditLineOption newCreditLine, int avoidAutoDescison) {
			return ExecuteSync<MainStrategy>(customerId, underwriterId, customerId, newCreditLine, avoidAutoDescison);
		} // MainStrategySync1

		public BoolActionResult GenerateMobileCode(string phone) {
			GenerateMobileCode strategyInstance;
			var result = ExecuteSync(out strategyInstance, null, null, phone);
			return new BoolActionResult { MetaData = result, Value = strategyInstance.IsError };
		} // GenerateMobileCode

		public BoolActionResult ValidateMobileCode(string phone, string code) {
			ValidateMobileCode strategyInstance;
			var result = ExecuteSync(out strategyInstance, null, null, phone, code);
			return new BoolActionResult { MetaData = result, Value = strategyInstance.IsValidatedSuccessfully() };
		} // ValidateMobileCode

		public WizardConfigsActionResult GetWizardConfigs() {
			GetWizardConfigs strategyInstance;

			var result = ExecuteSync(out strategyInstance, null, null);

			return new WizardConfigsActionResult {
				MetaData = result,
				IsSmsValidationActive = strategyInstance.IsSmsValidationActive,
				NumberOfMobileCodeAttempts = strategyInstance.NumberOfMobileCodeAttempts,
				AllowInsertingMobileCodeWithoutGeneration = strategyInstance.AllowInsertingMobileCodeWithoutGeneration
			};
		} // GetWizardConfigs

		public ActionMetaData UpdateCurrencyRates() {
			return Execute(null, null, typeof(UpdateCurrencyRates));
		} // UpdateCurrencyRates

		public QuickOfferActionResult QuickOffer(int customerId, bool saveOfferToDB) {
			try {
				return QuickOfferProcedure(customerId, saveOfferToDB);
			}
			catch (Exception e) {
				Log.Alert(e, "Exception during executing Quick offer strategy.");
				throw new FaultException(e.Message);
			} // try
		} // QuickOffer

		public QuickOfferActionResult QuickOfferWithPrerequisites(int customerId, bool saveOfferToDB) {
			try {
				Log.Debug("QuickOfferWithPrerequisites: performing company check for customer {0}...", customerId);
				new ExperianCompanyCheck(customerId, DB, Log).Execute();

				Log.Debug("QuickOfferWithPrerequisites: performing fraud check for customer {0}...", customerId);
				new FraudChecker(customerId, FraudMode.CompanyDetailsCheck, DB, Log).Execute();

				Log.Debug("QuickOfferWithPrerequisites: performing quick offer calculation for customer {0}...", customerId);
				return QuickOfferProcedure(customerId, saveOfferToDB);
			}
			catch (Exception e) {
				Log.Alert(e, "Exception during executing QuickOfferWithPrerequisites strategy.");
				throw new FaultException(e.Message);
			} // try
		} // QuickOfferWithPrerequisites

		#endregion IEzService exposed methods

		#region method IDisposable.Dispose

		public void Dispose() {
			Log.Msg("EzService instance disposed.");
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

		#region method Execute

		private ActionMetaData Execute(int? nCustomerID, int? nUserID, Type oStrategyType, params object[] args) {
			ActionMetaData amd = null;

			try {
				Log.Debug("Executing " + oStrategyType + " started...");

				amd = NewAsync(oStrategyType.ToString(), comment: string.Join("; ", args), nCustomerID: nCustomerID, nUserID: nUserID);

				var oParams = new List<object>(args) { DB, Log };

				ConstructorInfo oCreator = oStrategyType.GetConstructors().FirstOrDefault(ci => ci.GetParameters().Length == oParams.Count);

				if (oCreator == null)
					throw new Exception("Failed to find a constructor for " + oStrategyType + " with " + oParams.Count + " arguments.");

				Log.Debug(oStrategyType + " constructor found, invoking...");

				amd.UnderlyingThread = new Thread(() => {
					try {
						((AStrategy)oCreator.Invoke(oParams.ToArray())).Execute();

						Log.Debug("Executing " + oStrategyType + " complete.");

						SaveActionStatus(amd, ActionStatus.Done);
					}
					catch (Exception e) {
						Log.Alert(e, "Exception during executing " + oStrategyType + " strategy.");

						amd.Comment = e.Message;
						SaveActionStatus(amd, ActionStatus.Failed);
					} // try
				});

				SaveActionStatus(amd, ActionStatus.Launched);

				amd.UnderlyingThread.Start();

				Log.Debug("Executing {0} started on another thread [{1}].", oStrategyType, amd.UnderlyingThread.ManagedThreadId);

				return amd;
			}
			catch (Exception e) {
				if (amd != null) {
					amd.Comment = e.Message;
					SaveActionStatus(amd, ActionStatus.Failed);
				} // if

				Log.Alert(e, "Exception during executing " + oStrategyType + " strategy.");
				throw new FaultException(e.Message);
			} // try
		} // Execute

		#endregion method Execute

		#region method ExecuteSync

		private ActionMetaData ExecuteSync<T>(int? nCustomerID, int? nUserID, params object[] args) where T : AStrategy {
			T oInstance;
			return ExecuteSync<T>(out oInstance, nCustomerID, nUserID, args);
		} // ExecuteSync

		private ActionMetaData ExecuteSync<T>(out T oInstance, int? nCustomerID, int? nUserID, params object[] args) where T : AStrategy {
			ActionMetaData amd = null;

			try {
				string sStrategyType = typeof(T).ToString();

				Log.Debug("Executing " + sStrategyType + " started in sync...");

				amd = NewSync(sStrategyType, comment: string.Join("; ", args), nCustomerID: nCustomerID, nUserID: nUserID);

				var oParams = new List<object>(args) { DB, Log };

				ConstructorInfo oCreator =
					typeof(T).GetConstructors().FirstOrDefault(ci => ci.GetParameters().Length == oParams.Count);

				if (oCreator == null)
					throw new Exception("Failed to find a constructor for " + sStrategyType + " with " + oParams.Count + " arguments.");

				Log.Debug(sStrategyType + " constructor found, invoking...");

				oInstance = (T)oCreator.Invoke(oParams.ToArray());

				Log.Debug(sStrategyType + " constructor complete, executing...");

				oInstance.Execute();

				Log.Debug("Executing " + sStrategyType + " complete in sync.");

				SaveActionStatus(amd, ActionStatus.Done);

				return amd;
			}
			catch (Exception e) {
				if (amd != null) {
					amd.Comment = e.Message;
					SaveActionStatus(amd, ActionStatus.Failed);
				} // if

				Log.Alert(e, "Exception during executing " + typeof(T) + " strategy.");
				throw new FaultException(e.Message);
			} // try
		} // ExecuteSync

		#endregion method ExecuteSync

		#region method SaveActionStatus

		private void SaveActionStatus(ActionMetaData amd, ActionStatus nNewStatus) {
			if (amd == null)
				return;

			amd.Save(nNewStatus);

			if (amd.IsComplete() == TriState.Yes) {
				lock (ms_oLockActiveActions) {
					if (ms_oActiveActions.ContainsKey(amd.ActionID))
						ms_oActiveActions.Remove(amd.ActionID);
				} // lock
			} // if
		} // SaveActionStatus

		#endregion method SaveActionStatus

		#region method NewAsync

		private ActionMetaData NewAsync(string sActionName, ActionStatus status = ActionStatus.InProgress, string comment = null, int? nCustomerID = null, int? nUserID = null) {
			return CreateActionMetaData(sActionName, false, status, comment, nCustomerID, nUserID);
		} // NewAsync

		#endregion method NewAsync

		#region method NewSync

		private ActionMetaData NewSync(string sActionName, ActionStatus status = ActionStatus.InProgress, string comment = null, int? nCustomerID = null, int? nUserID = null) {
			return CreateActionMetaData(sActionName, true, status, comment, nCustomerID, nUserID);
		} // NewSync

		#endregion method NewSync

		#region method CreateActionMetaData

		private ActionMetaData CreateActionMetaData(string sActionName, bool bIsSynchronous, ActionStatus status, string comment, int? nCustomerID, int? nUserID) {
			var amd = ActionMetaData.Create(InstanceID, sActionName, DB, Log, bIsSynchronous, status, comment, nCustomerID, nUserID);

			if (amd.IsComplete() != TriState.Yes) {
				lock (ms_oLockActiveActions) {
					ms_oActiveActions[amd.ActionID] = amd;
				} // lock
			} // if

			return amd;
		} // CreateActionMetaData

		#endregion method CreateActionMetaData

		#region method QuickOfferProcedure

		private QuickOfferActionResult QuickOfferProcedure(int nCustomerID, bool bSaveOfferToDB) {
			QuickOffer oStrategy;

			var oResult = ExecuteSync(out oStrategy, nCustomerID, nCustomerID, nCustomerID, bSaveOfferToDB);

			return new QuickOfferActionResult {
				HasValue = !ReferenceEquals(oStrategy.Offer, null),
				Value = oStrategy.Offer,
				MetaData = oResult,
			};
		} // QuickOfferProcedure

		#endregion method QuickOfferProcedure

		#region properties and fields

		private readonly EzServiceInstanceRuntimeData m_oData;

		#region property Log

		private ASafeLog Log {
			get {
				if (ReferenceEquals(m_oTheLog, null))
					m_oTheLog = ((m_oData != null) && (m_oData.Log != null)) ? m_oData.Log : new SafeLog();

				return m_oTheLog;
			} // get
		} // Log
		private ASafeLog m_oTheLog;

		#endregion property Log

		#region property DB

		private AConnection DB {
			get { return ReferenceEquals(m_oData, null) ? null : m_oData.DB; } // get
		} // DB

		#endregion property DB

		#region property InstanceName

		private string InstanceName {
			get { return ReferenceEquals(m_oData, null) ? string.Empty : m_oData.InstanceName; } // get
		} // InstanceName

		#endregion property InstanceName

		#region property InstanceID

		private int InstanceID {
			get { return ReferenceEquals(m_oData, null) ? 0 : m_oData.InstanceID; } // get
		} // InstanceID

		#endregion property InstanceID

		#region static fields

		private static readonly SortedDictionary<Guid, ActionMetaData> ms_oActiveActions;
		private static readonly object ms_oLockActiveActions;
		private static readonly object ms_oLockTerminateAction;

		private static bool ms_bNewInstancesAllowed;
		private static readonly object ms_oLockNewInstancesAllowed;

		#endregion static fields

		#endregion properties and fields

		#endregion private
	} // class EzServiceImplementation
} // namespace EzService
