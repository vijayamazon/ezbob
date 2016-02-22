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
	using log4net;

	[ServiceBehavior(
		InstanceContextMode = InstanceContextMode.PerCall,
		IncludeExceptionDetailInFaults = true
	)]
	public partial class EzServiceImplementation : IEzServiceAdmin, IEzService, IDisposable {
		static EzServiceImplementation() {
			activeActions = new SortedDictionary<Guid, ActionMetaData>();
			lockActiveActions = new object();
			lockTerminateAction = new object();

			lockNewInstancesAllowed = new object();
			newInstancesAllowed = true;
		} // static constructor

		public EzServiceImplementation(EzServiceInstanceRuntimeData oData) {
			if (!NewInstancesAllowed)
				throw new FaultException("Cannot create " + InstanceName + " instance: new instances are disabled.");

			this.data = oData;

			Ezbob.Backend.Strategies.Library.Initialize(
				ReferenceEquals(this.data, null) ? null : this.data.Env,
				ReferenceEquals(this.data, null) ? null : this.data.DB,
				((this.data != null) && (this.data.Log != null)) ? this.data.Log : new SafeLog()
			);

			Log.Msg("{0} instance created.", InstanceName);
		} // constructor

		public void Dispose() {
			Log.Msg("EzService instance disposed.");
		} // Dispose

		public ASafeLog Log {
			get {
				if (ReferenceEquals(m_oTheLog, null))
					m_oTheLog = ((this.data != null) && (this.data.Log != null)) ? this.data.Log : new SafeLog();

				return m_oTheLog;
			} // get
		} // Log

		private ASafeLog m_oTheLog;

		public AConnection DB {
			get { return ReferenceEquals(this.data, null) ? null : this.data.DB; } // get
		} // DB

		public ActionMetaData Execute<TStrategy>(
			int? nCustomerID,
			int? nUserID,
			params object[] args
		) where TStrategy : AStrategy {
			try {
				return Execute(new ExecuteArguments(args) {
					StrategyType = typeof (TStrategy),
					CustomerID = nCustomerID,
					UserID = nUserID,
				});
			} catch (FaultException) {
				throw;
			} catch (Exception e) {
				Log.Alert(e, "Failed to create ExecuteArguments.");
				throw new FaultException("Failed to create ExecuteArguments: " + e.Message);
			} // try
		} // Execute

		// async
		public ActionMetaData Execute(ExecuteArguments oArgs) {
			ActionMetaData amd = null;

			try {
				if (oArgs == null)
					throw new Alert(Log, "No ExecuteArguments specified.");

				SetLogThreadProperties(oArgs);
				Log.Debug("Executing " + oArgs.StrategyType + " started...");

				amd = NewAsync(
					oArgs.StrategyType.ToString(),
					comment: oArgs.StrategyArgumentsStr,
					nCustomerID: oArgs.CustomerID,
					nUserID: oArgs.UserID
				);

				ConstructorInfo oCreator = oArgs.StrategyType.GetConstructors().FirstOrDefault(
					ci => ci.GetParameters().Length == oArgs.StrategyArguments.Count
				);

				if (oCreator == null) {
					string msg = string.Format(
						"Failed to find a constructor for {0} with {1} arguments.",
						oArgs.StrategyType ,
						oArgs.StrategyArguments.Count
					);

					throw new Alert(Log, msg);
				} // if

				Log.Debug(oArgs.StrategyType + " constructor found, invoking...");

				amd.UnderlyingThread = new Thread(() => {
					try {
						SetLogThreadProperties(oArgs);
						AStrategy oInstance = (AStrategy)oCreator.Invoke(oArgs.StrategyArguments.ToArray());

						if (oInstance == null)
							throw new NullReferenceException("Failed to create an instance of " + oArgs.StrategyType);

						amd.Strategy = oInstance;

						oInstance.Context.UserID = oArgs.UserID;
						oInstance.Context.CustomerID = oArgs.CustomerID;

						if (oArgs.OnInit != null) {
							Log.Debug(oInstance.Name + " instance created, invoking an initialization action...");

							oArgs.OnInit(oInstance, amd);

							Log.Debug(oInstance.Name + " initialization action complete.");
						} // if

						Log.Debug(oInstance.Name + " instance is initialized, executing...");

						oInstance.Execute();

						Log.Debug("Executing " + oArgs.StrategyType + " complete.");

						SaveActionStatus(amd, ActionStatus.Done);

						if (oArgs.OnSuccess != null) {
							Log.Debug(oInstance.Name + " instance running complete, invoking an OnSuccess action...");

							oArgs.OnSuccess(oInstance, amd);

							Log.Debug(oInstance.Name + " OnSuccess action complete.");
						} // if
					} catch (Exception e) {
						Log.Alert(e, "Exception during executing " + oArgs.StrategyType + " strategy.");

						amd.Comment = e.Message;
						SaveActionStatus(amd, ActionStatus.Failed);

						if (oArgs.OnFail != null) {
							Log.Debug(oArgs.StrategyType + " instance running failed, invoking an OnFail action...");

							try {
								oArgs.OnFail(amd);
								Log.Debug(oArgs.StrategyType + " OnFail action complete.");
							} catch (Exception ie) {
								Log.Alert(
									ie,
									"Exception during executing of OnFail handler of " + oArgs.StrategyType + " strategy."
								);
							} // try
						} // if
					} // try
				});

				SaveActionStatus(amd, ActionStatus.Launched);

				if (oArgs.OnLaunch != null) {
					Log.Debug(oArgs.StrategyType + " instance is to be launched, invoking an OnLaunch action...");

					oArgs.OnLaunch(amd);

					Log.Debug(oArgs.StrategyType + " OnLaunch action complete.");
				} // if

				amd.UnderlyingThread.Start();

				Log.Debug(
					"Executing {0} started on another thread [{1}].",
					oArgs.StrategyType,
					amd.UnderlyingThread.ManagedThreadId
				);

				return amd;
			} catch (Exception e) {
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
					} catch (Exception ie) {
						Log.Alert(
							ie,
							"Exception during executing of OnException handler of " + oArgs.StrategyType + " strategy."
						);
					} // try
				} // if

				throw new FaultException(e.Message);
			} // try
		} // Execute

		private ActionMetaData ExecuteSync<T>(int? nCustomerID, int? nUserID, params object[] args) where T : AStrategy {
			T oInstance;
			return ExecuteSync(out oInstance, nCustomerID, nUserID, args);
		} // ExecuteSync

		/// <summary>
		/// 
		/// 
		/// </summary>
		/// <typeparam name="T">type of Strategy to execute</typeparam>
		/// <param name="oInstance">output instance of executed strategy</param>
		/// <param name="nCustomerID">customer ID null-able</param>
		/// <param name="nUserID">underwriterID, null-able</param>
		/// <param name="args">Strategy constructor parameters</param>
		/// <returns></returns>
		private ActionMetaData ExecuteSync<T>(
			out T oInstance,
			int? nCustomerID,
			int? nUserID,
			params object[] args
		) where T : AStrategy {
			return ExecuteSync(out oInstance, new ExecuteArguments(args) { CustomerID = nCustomerID, UserID = nUserID, });
		} // ExecuteSync

		private ActionMetaData ExecuteSync<T>(ExecuteArguments args) where T : AStrategy {
			T oInstance;
			return ExecuteSync<T>(out oInstance, args);
		} // ExecuteSync

		private ActionMetaData ExecuteSync<T>(out T oInstance, ExecuteArguments args) where T : AStrategy {
			ActionMetaData amd = null;

			try {
				string sStrategyType = typeof(T).ToString();

				if (args == null)
					throw new Alert(Log, "No strategy arguments specified for " + sStrategyType + ".");

				args.StrategyType = typeof (T); // just to avoid question which type is used.

				SetLogThreadProperties(args);

				Log.Debug("Executing " + sStrategyType + " started in sync...");

				amd = NewSync(
					sStrategyType,
					comment: args.StrategyArgumentsStr,
					nCustomerID: args.CustomerID,
					nUserID: args.UserID
				);

				ConstructorInfo oCreator = typeof(T).GetConstructors().FirstOrDefault(
					ci => ci.GetParameters().Length == args.StrategyArguments.Count
				);

				if (oCreator == null) {
					string msg = string.Format(
						"Failed to find a constructor for {0} with {1} arguments.",
						sStrategyType,
						args.StrategyArguments.Count
					);

					throw new Alert(Log, msg);
				} // if

				Log.Debug(sStrategyType + " constructor found, invoking...");

				oInstance = (T)oCreator.Invoke(args.StrategyArguments.ToArray());

				if (oInstance == null)
					throw new NullReferenceException("Failed to create an instance of " + sStrategyType);

				amd.Strategy = oInstance;

				oInstance.Context.UserID = args.UserID;
				oInstance.Context.CustomerID = args.CustomerID;

				if (args.OnInit != null) {
					Log.Debug(oInstance.Name + " instance created, invoking an initialization action...");

					args.OnInit(oInstance, amd);

					Log.Debug(oInstance.Name + " initialization action complete.");
				} // if

				if (args.OnLaunch != null) {
					Log.Debug(args.StrategyType + " instance is to be launched, invoking an OnLaunch action...");

					args.OnLaunch(amd);

					Log.Debug(args.StrategyType + " OnLaunch action complete.");
				} // if

				Log.Debug(oInstance.Name + " instance is initialized, executing...");

				oInstance.Execute();

				Log.Debug("Executing " + oInstance.Name + " complete in sync.");

				SaveActionStatus(amd, ActionStatus.Done);

				if (args.OnSuccess != null) {
					Log.Debug(oInstance.Name + " instance running complete, invoking an OnSuccess action...");

					args.OnSuccess(oInstance, amd);

					Log.Debug(oInstance.Name + " OnSuccess action complete.");
				} // if

				return amd;
			} catch (Exception e) {
				Exception mostInnerException = e;

				while (mostInnerException.InnerException != null)
					mostInnerException = mostInnerException.InnerException;

				if (amd != null) {
					amd.Comment += " Exception caught: " + mostInnerException.Message;
					SaveActionStatus(amd, ActionStatus.Failed);
				} // if

				if (!(e is AException))
					Log.Alert(mostInnerException, "Exception during executing " + typeof(T) + " strategy.");

				if ((args != null) && (args.OnException != null)) {
					Log.Debug(args.StrategyType + " instance running failed, invoking an OnException action...");

					try {
						args.OnException(amd);
						Log.Debug(args.StrategyType + " OnException action complete.");
					} catch (Exception ie) {
						Log.Alert(
							ie,
							"Exception during executing of OnException handler of " + args.StrategyType + " strategy."
						);
					} // try
				} // if

				throw new FaultException(mostInnerException.Message);
			} // try
		} // ExecuteSync

		private static bool NewInstancesAllowed {
			get {
				bool b;

				lock (lockNewInstancesAllowed)
					b = newInstancesAllowed;

				return b;
			} // get
			set {
				lock (lockNewInstancesAllowed)
					newInstancesAllowed = value;
			} // set
		} // NewInstancesAllowed

		private void SaveActionStatus(ActionMetaData amd, ActionStatus nNewStatus) {
			if (amd == null)
				return;

			amd.Save(nNewStatus);

			if (amd.IsComplete() == TriState.Yes) {
				lock (lockActiveActions) {
					if (activeActions.ContainsKey(amd.ActionID))
						activeActions.Remove(amd.ActionID);
				} // lock
			} // if
		} // SaveActionStatus

		private ActionMetaData NewAsync(
			string sActionName,
			ActionStatus status = ActionStatus.InProgress,
			string comment = null,
			int? nCustomerID = null,
			int? nUserID = null
		) {
			return CreateActionMetaData(sActionName, false, status, comment, nCustomerID, nUserID);
		} // NewAsync

		private ActionMetaData NewSync(
			string sActionName,
			ActionStatus status = ActionStatus.InProgress,
			string comment = null,
			int? nCustomerID = null,
			int? nUserID = null
		) {
			return CreateActionMetaData(sActionName, true, status, comment, nCustomerID, nUserID);
		} // NewSync

		private ActionMetaData CreateActionMetaData(
			string sActionName,
			bool bIsSynchronous,
			ActionStatus status,
			string comment,
			int? nCustomerID,
			int? nUserID
		) {
			var amd = ActionMetaData.Create(
				InstanceID,
				sActionName,
				DB,
				Log,
				bIsSynchronous,
				status,
				comment,
				nCustomerID,
				nUserID
			);

			if (amd.IsComplete() != TriState.Yes) {
				lock (lockActiveActions) {
					activeActions[amd.ActionID] = amd;
				} // lock
			} // if

			return amd;
		} // CreateActionMetaData

		private string InstanceName {
			get { return ReferenceEquals(this.data, null) ? string.Empty : this.data.InstanceName; }
		} // InstanceName

		private int InstanceID {
			get { return ReferenceEquals(this.data, null) ? 0 : this.data.InstanceID; }
		} // InstanceID

		private void SetLogThreadProperties(ExecuteArguments args) {
			ThreadContext.Properties["UserId"] = args.UserID;
			ThreadContext.Properties["CustomerId"] = args.CustomerID;
			ThreadContext.Properties["StrategyType"] = args.StrategyType.Name;
		} // SetLogThreadProperties

		private readonly EzServiceInstanceRuntimeData data;

		private static readonly SortedDictionary<Guid, ActionMetaData> activeActions;
		private static readonly object lockActiveActions;
		private static readonly object lockTerminateAction;

		private static bool newInstancesAllowed;
		private static readonly object lockNewInstancesAllowed;
	} // class EzServiceImplementation
} // namespace EzService
