namespace EzService.EzServiceImplementation {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.ServiceModel;
	using System.Threading;
	using Ezbob.Backend.Strategies;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Exceptions;

	[ServiceBehavior(
		InstanceContextMode = InstanceContextMode.PerCall,
		IncludeExceptionDetailInFaults = true
	)]
	public partial class EzServiceImplementation : IEzServiceAdmin, IEzService, IDisposable {

		static EzServiceImplementation() {
			ms_oActiveActions = new SortedDictionary<Guid, ActionMetaData>();
			ms_oLockActiveActions = new object();
			ms_oLockTerminateAction = new object();

			ms_oLockNewInstancesAllowed = new object();
			ms_bNewInstancesAllowed = true;
		} // static constructor

		public EzServiceImplementation(EzServiceInstanceRuntimeData oData) {
			if (!NewInstancesAllowed)
				throw new FaultException("Cannot create " + InstanceName + " instance: new instances are disabled.");

			m_oData = oData;

			Ezbob.Backend.Strategies.Library.Initialize(
				ReferenceEquals(m_oData, null) ? null : m_oData.Env,
				ReferenceEquals(m_oData, null) ? null : m_oData.DB,
				((m_oData != null) && (m_oData.Log != null)) ? m_oData.Log : new SafeLog()
			);

			Log.Msg("{0} instance created.", InstanceName);
		} // constructor

		public void Dispose() {
			Log.Msg("EzService instance disposed.");
		} // Dispose

		public ASafeLog Log {
			get {
				if (ReferenceEquals(m_oTheLog, null))
					m_oTheLog = ((m_oData != null) && (m_oData.Log != null)) ? m_oData.Log : new SafeLog();

				return m_oTheLog;
			} // get
		} // Log

		private ASafeLog m_oTheLog;

		public AConnection DB {
			get { return ReferenceEquals(m_oData, null) ? null : m_oData.DB; } // get
		} // DB

		public ActionMetaData Execute<TStrategy>(int? nCustomerID, int? nUserID, params object[] args) where TStrategy : AStrategy {
			try {
				return Execute(new ExecuteArguments(args) {
					StrategyType = typeof (TStrategy),
					CustomerID = nCustomerID,
					UserID = nUserID,
				});
			}
			catch (FaultException) {
				throw;
			}
			catch (Exception e) {
				Log.Alert(e, "Failed to create ExecuteArguments.");
				throw new FaultException("Failed to create ExecuteArguments: " + e.Message);
			} // try
		} // Execute

		public ActionMetaData Execute(ExecuteArguments oArgs) {
			ActionMetaData amd = null;

			try {
				if (oArgs == null)
					throw new Alert(Log, "No ExecuteArguments specified.");

				Log.Debug("Executing " + oArgs.StrategyType + " started...");

				amd = NewAsync(
					oArgs.StrategyType.ToString(),
					comment: oArgs.StrategyArgumentsStr,
					nCustomerID: oArgs.CustomerID,
					nUserID: oArgs.UserID
				);

				ConstructorInfo oCreator = oArgs.StrategyType.GetConstructors().FirstOrDefault(ci => ci.GetParameters().Length == oArgs.StrategyArguments.Count);

				if (oCreator == null)
					throw new Alert(Log, "Failed to find a constructor for " + oArgs.StrategyType + " with " + oArgs.StrategyArguments.Count + " arguments.");

				Log.Debug(oArgs.StrategyType + " constructor found, invoking...");

				amd.UnderlyingThread = new Thread(() => {
					try {
						AStrategy oInstance = (AStrategy)oCreator.Invoke(oArgs.StrategyArguments.ToArray());

						if (oArgs.OnInit != null) {
							Log.Debug(oInstance.Name + " instance created, invoking an initialisation action...");

							oArgs.OnInit(oInstance, amd);

							Log.Debug(oInstance.Name + " initialisation action complete.");
						} // if

						Log.Debug(oInstance.Name + " instance is initialised, executing...");

						oInstance.Execute();

						Log.Debug("Executing " + oArgs.StrategyType + " complete.");

						SaveActionStatus(amd, ActionStatus.Done);

						if (oArgs.OnSuccess != null) {
							Log.Debug(oInstance.Name + " instance running complete, invoking an OnSuccess action...");

							oArgs.OnSuccess(oInstance, amd);

							Log.Debug(oInstance.Name + " OnSuccess action complete.");
						} // if
					}
					catch (Exception e) {
						Log.Alert(e, "Exception during executing " + oArgs.StrategyType + " strategy.");

						amd.Comment = e.Message;
						SaveActionStatus(amd, ActionStatus.Failed);

						if (oArgs.OnFail != null) {
							Log.Debug(oArgs.StrategyType + " instance running failed, invoking an OnFail action...");

							try {
								oArgs.OnFail(amd);
								Log.Debug(oArgs.StrategyType + " OnFail action complete.");
							}
							catch (Exception ie) {
								Log.Alert(ie, "Exception during executing of OnFail handler of " + oArgs.StrategyType + " strategy.");
							} // try
						} // if
					} // try
				});

				SaveActionStatus(amd, ActionStatus.Launched);

				if (oArgs.OnLaunch != null) {
					Log.Debug(oArgs.StrategyType + " instance is to be launched, invoking an OnLaunch action...");

					try {
						oArgs.OnLaunch(amd);
						Log.Debug(oArgs.StrategyType + " OnLaunch action complete.");
					}
					catch (Exception ie) {
						Log.Alert(ie, "Exception during executing of OnLaunch handler of " + oArgs.StrategyType + " strategy.");
					} // try
				} // if

				amd.UnderlyingThread.Start();

				Log.Debug("Executing {0} started on another thread [{1}].", oArgs.StrategyType, amd.UnderlyingThread.ManagedThreadId);

				return amd;
			}
			catch (Exception e) {
				if (amd != null) {
					amd.Comment += " Exception caught: " + e.Message;
					SaveActionStatus(amd, ActionStatus.Failed);
				} // if

				if (!(e is AException)) {
					string sStrategyType = oArgs == null ? "UNKNOWN" : oArgs.StrategyType.ToString();
					Log.Alert(e, "Exception during executing " + sStrategyType + " strategy.");
				} // if

				if ((oArgs != null) && (oArgs.OnException != null)) {
					try {
						oArgs.OnException(amd);
					}
					catch (Exception ie) {
						Log.Alert(ie, "Exception during executing of OnException handler of " + oArgs.StrategyType + " strategy.");
					} // try
				} // if

				throw new FaultException(e.Message);
			} // try
		} // Execute

		private ActionMetaData ExecuteSync<T>(int? nCustomerID, int? nUserID, params object[] args) where T : AStrategy {
			T oInstance;
			return ExecuteSync(out oInstance, nCustomerID, nUserID, args);
		} // ExecuteSync

		private ActionMetaData ExecuteSync<T>(out T oInstance, int? nCustomerID, int? nUserID, params object[] args) where T : AStrategy {
			return ExecuteSync(out oInstance, nCustomerID, nUserID, null, args);
		} // ExecuteSync

		private ActionMetaData ExecuteSync<T>(out T oInstance, int? nCustomerID, int? nUserID, Action<T> oInitAction, params object[] args) where T : AStrategy {
			ActionMetaData amd = null;

			try {
				string sStrategyType = typeof(T).ToString();

				Log.Debug("Executing " + sStrategyType + " started in sync...");

				amd = NewSync(sStrategyType, comment: string.Join("; ", args), nCustomerID: nCustomerID, nUserID: nUserID);

				ConstructorInfo oCreator =
					typeof(T).GetConstructors().FirstOrDefault(ci => ci.GetParameters().Length == args.Length);

				if (oCreator == null)
					throw new Alert(Log, "Failed to find a constructor for " + sStrategyType + " with " + args.Length + " arguments.");

				Log.Debug(sStrategyType + " constructor found, invoking...");

				oInstance = (T)oCreator.Invoke(args.ToArray());

				if (oInstance == null)
					throw new NullReferenceException("Failed to create an instance of " + sStrategyType);

				if (oInitAction != null) {
					Log.Debug(oInstance.Name + " instance created, invoking an initialisation action...");

					oInitAction(oInstance);

					Log.Debug(oInstance.Name + " initialisation action complete.");
				} // if

				Log.Debug(oInstance.Name + " instance is initialised, executing...");

				oInstance.Execute();

				Log.Debug("Executing " + oInstance.Name + " complete in sync.");

				SaveActionStatus(amd, ActionStatus.Done);

				return amd;
			}
			catch (Exception e) {
				Exception mostInnerException = e;
				while (mostInnerException.InnerException != null)
					mostInnerException = mostInnerException.InnerException;

				if (amd != null) {
					amd.Comment += " Exception caught: " + mostInnerException.Message;
					SaveActionStatus(amd, ActionStatus.Failed);
				} // if

				if (!(e is AException))
					Log.Alert(mostInnerException, "Exception during executing " + typeof(T) + " strategy.");

				throw new FaultException(mostInnerException.Message);
			} // try
		} // ExecuteSync

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

		private ActionMetaData NewAsync(string sActionName, ActionStatus status = ActionStatus.InProgress, string comment = null, int? nCustomerID = null, int? nUserID = null) {
			return CreateActionMetaData(sActionName, false, status, comment, nCustomerID, nUserID);
		} // NewAsync

		private ActionMetaData NewSync(string sActionName, ActionStatus status = ActionStatus.InProgress, string comment = null, int? nCustomerID = null, int? nUserID = null) {
			return CreateActionMetaData(sActionName, true, status, comment, nCustomerID, nUserID);
		} // NewSync

		private ActionMetaData CreateActionMetaData(string sActionName, bool bIsSynchronous, ActionStatus status, string comment, int? nCustomerID, int? nUserID) {
			var amd = ActionMetaData.Create(InstanceID, sActionName, DB, Log, bIsSynchronous, status, comment, nCustomerID, nUserID);

			if (amd.IsComplete() != TriState.Yes) {
				lock (ms_oLockActiveActions) {
					ms_oActiveActions[amd.ActionID] = amd;
				} // lock
			} // if

			return amd;
		} // CreateActionMetaData

		private readonly EzServiceInstanceRuntimeData m_oData;

		private string InstanceName {
			get { return ReferenceEquals(m_oData, null) ? string.Empty : m_oData.InstanceName; } // get
		} // InstanceName

		private int InstanceID {
			get { return ReferenceEquals(m_oData, null) ? 0 : m_oData.InstanceID; } // get
		} // InstanceID

		private static readonly SortedDictionary<Guid, ActionMetaData> ms_oActiveActions;
		private static readonly object ms_oLockActiveActions;
		private static readonly object ms_oLockTerminateAction;

		private static bool ms_bNewInstancesAllowed;
		private static readonly object ms_oLockNewInstancesAllowed;

	} // class EzServiceImplementation
} // namespace EzService
