using System;
using System.Runtime.Serialization;
using System.Threading;
using Ezbob.Database;
using Ezbob.Logger;

namespace EzService {
	#region enum TriState

	public enum TriState {
		Unknown,
		Yes,
		No
	} // TriState

	#endregion enum TriState

	#region enum ActionStatus

	/// <summary>
	/// Do not change numeric values - they are used as keys in DB.
	/// </summary>
	[DataContract]
	public enum ActionStatus {
		[EnumMember]
		InProgress = 1,

		/// <summary>
		/// Action completed successfully.
		/// </summary>
		[EnumMember]
		Done = 2,

		/// <summary>
		/// Action completed, but result is "failure".
		/// </summary>
		[EnumMember]
		Finished = 3,

		/// <summary>
		/// Action failed to complete.
		/// </summary>
		[EnumMember]
		Failed = 4,

		/// <summary>
		/// Action was terminated.
		/// </summary>
		[EnumMember]
		Terminated = 5,

		[EnumMember]
		Unknown = 6,

		/// <summary>
		/// Underlying thread has been started in background.
		/// </summary>
		[EnumMember]
		Launched = 7,
	} // enum ActionStatus

	#endregion enum ActionStatus

	#region class ActionMetaData

	[DataContract]
	public class ActionMetaData {
		#region public

		#region static methods

		#region method Create

		public static ActionMetaData Create(int nServiceInstanceID, string sActionName, AConnection oDB, ASafeLog oLog, bool bIsSynchronous, ActionStatus nStatus, string sComment) {
			Guid oActionID = Guid.NewGuid();

			while (oActionID == Guid.Empty)
				oActionID = Guid.NewGuid();

			var amd = new ActionMetaData {
				ActionID = oActionID,
				Name = sActionName,
				IsSynchronous = bIsSynchronous,
				Status = nStatus,
				UnderlyingThread = Thread.CurrentThread,
				Comment = sComment,
				m_oDB = oDB,
				m_oLog = oLog,
				m_nServiceInstanceID = nServiceInstanceID
			};

			amd.Save();

			return amd;
		} // Create

		#endregion method Create

		#endregion static methods

		#region serialised properties

		#region property ActionID

		[DataMember]
		public Guid ActionID { get; private set; }

		#endregion property ActionID

		#region property Name

		[DataMember]
		public string Name { get; private set; }

		#endregion property Name

		#region property Status

		[DataMember]
		public ActionStatus Status {
			get {
				lock (m_oLockStatus)
					return m_nStatus;
			} // get
			set {
				lock (m_oLockStatus)
					m_nStatus = value;
			} // set
		} // Status

		private ActionStatus m_nStatus;

		#endregion property Status

		#region property Comment

		[DataMember]
		public string Comment {
			get {
				lock (m_oLockComment)
					return m_sComment;
			} // get
			set {
				lock (m_oLockComment)
					m_sComment = value;
			} // set
		} // Status

		private string m_sComment;

		#endregion property Comment

		#region property IsSynchronous

		[DataMember]
		public bool IsSynchronous { get; private set; }

		#endregion property IsSynchronous

		#endregion serialised properties

		#region method ToString

		public override string ToString() {
			return string.Format(
				"{{ {0}: {5} [{1}sync {2}] {3}: {4} }}",
				ActionID,
				IsSynchronous ? "" : "a",
				UnderlyingThread.ManagedThreadId,
				Status,
				Comment ?? "-- no comments --",
				Name
			);
		} // ToString

		#endregion method ToString

		#region method IsComplete

		public TriState IsComplete() {
			switch (Status) {
			case ActionStatus.Done:
			case ActionStatus.Finished:
			case ActionStatus.Failed:
			case ActionStatus.Terminated:
				return TriState.Yes;

			case ActionStatus.InProgress:
			case ActionStatus.Launched:
				return TriState.No;

			case ActionStatus.Unknown:
				return TriState.Unknown;

			default:
				throw new ArgumentOutOfRangeException();
			} // switch
		} // IsComplete

		#endregion method IsComplete

		#region property UnderlyingThread

		public Thread UnderlyingThread {
			get { return m_oUnderlyingThread; } // get
			set {
				if (ReferenceEquals(value, null))
					throw new ArgumentNullException("value", "UnderlyingThread cannot be null");

				m_oUnderlyingThread = value;
			} // set
		} // UnderlyingThread

		private Thread m_oUnderlyingThread;

		#endregion property UnderlyingThread

		#region method Save

		public void Save(ActionStatus nNewStatus) {
			Status = nNewStatus;
			Save();
		} // Save

		public void Save() {
			lock (m_oLockSave) {
				m_oLog.Debug("Saving action status of {0} to DB...", this);

				try {
					m_oDB.ExecuteNonQuery("EzServiceSaveActionMetaData",
						CommandSpecies.StoredProcedure,
						new QueryParameter("@InstanceID", m_nServiceInstanceID),
						new QueryParameter("@ActionName", Name),
						new QueryParameter("@ActionID", ActionID),
						new QueryParameter("@IsSync", IsSynchronous),
						new QueryParameter("@Status", (int)Status),
						new QueryParameter("@CurrentThreadID", Thread.CurrentThread.ManagedThreadId),
						new QueryParameter("@UnderlyingThreadID", UnderlyingThread.ManagedThreadId),
						new QueryParameter("@Comment", Comment)
					);

					m_oLog.Debug("Saving action status of {0} to DB complete.", this);
				}
				catch (Exception e) {
					m_oLog.Alert(e, "Failed to save action status of {0} to DB.", this);
				} // try
			} // lock
		} // Save

		#endregion method Save

		#endregion public

		#region private

		#region constructor

		private ActionMetaData() {
			m_oLockSave = new object();
			m_oLockStatus = new object();
			m_oLockComment = new object();
		} // constructor

		#endregion constructor

		#region locks

		private readonly object m_oLockSave;
		private readonly object m_oLockStatus;
		private readonly object m_oLockComment;

		#endregion locks

		#region fields

		private AConnection m_oDB;
		private ASafeLog m_oLog;
		private int m_nServiceInstanceID;

		#endregion fields

		#endregion private
	} // struct ActionMetaData

	#endregion class ActionMetaData
} // namespace EzService
