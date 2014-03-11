﻿namespace EzService {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.ServiceModel;
	using System.Threading;
	using EzBob.Backend.Strategies;
	using Ezbob.Database;
	using Ezbob.Logger;

	[ServiceBehavior(
		InstanceContextMode = InstanceContextMode.PerCall,
		IncludeExceptionDetailInFaults = true
	)]
	public partial class EzServiceImplementation : IEzServiceAdmin, IEzService, IDisposable {
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

		private ActionMetaData ExecuteSyncParamsAtEnd<T>(out T oInstance, int? nCustomerID, int? nUserID, params object[] args) where T : AStrategy
		{
			ActionMetaData amd = null;

			try
			{
				string sStrategyType = typeof(T).ToString();

				Log.Debug("Executing " + sStrategyType + " started in sync...");

				amd = NewSync(sStrategyType, comment: string.Join("; ", args), nCustomerID: nCustomerID, nUserID: nUserID);

				var oParams = new List<object>();
				oParams.Add(DB);
				oParams.Add(Log);
				oParams.AddRange(args);

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
			catch (Exception e)
			{
				if (amd != null)
				{
					amd.Comment = e.Message;
					SaveActionStatus(amd, ActionStatus.Failed);
				} // if

				Log.Alert(e, "Exception during executing " + typeof(T) + " strategy.");
				throw new FaultException(e.Message);
			} // try
		} // ExecuteSyncParamsAtEnd

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
