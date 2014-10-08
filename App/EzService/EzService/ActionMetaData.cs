namespace EzService {
	using System;
	using System.Runtime.Serialization;
	using System.Threading;
	using Ezbob.Database;
	using Ezbob.Logger;

	#region enum TriState

	public enum TriState {
		Unknown,
		Yes,
		No
	} // TriState

	#endregion enum TriState

	#region class ActionMetaData

	[DataContract]
	public class ActionMetaData {
		#region public

		#region static methods

		#region method Create

		public static ActionMetaData Create(int nServiceInstanceID, string sActionName, AConnection oDB, ASafeLog oLog, bool bIsSynchronous, ActionStatus nStatus, string sComment, int? nCustomerID, int? nUserID) {
			Guid oActionID = Guid.NewGuid();

			while (oActionID == Guid.Empty)
				oActionID = Guid.NewGuid();

			var amd = new ActionMetaData(nStatus) {
				ActionID = oActionID,
				Name = sActionName,
				IsSynchronous = bIsSynchronous,
				UnderlyingThread = Thread.CurrentThread,
				Comment = sComment,
				m_oDB = oDB,
				m_oLog = oLog,
				m_nServiceInstanceID = nServiceInstanceID,
				UserID = nUserID,
				CustomerID = nCustomerID
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
			get { return m_oStatus.Value; } // get
			set { m_oStatus.Value = value; } // set
		} // Status

		private readonly SafeValue<ActionStatus> m_oStatus;

		#endregion property Status

		#region property Comment

		[DataMember]
		public string Comment {
			get { return m_oComment.Value; } // get
			set { m_oComment.Value = value; } // set
		} // Status

		private readonly SafeValue<string> m_oComment;

		#endregion property Comment

		#region property IsSynchronous

		[DataMember]
		public bool IsSynchronous { get; private set; }

		#endregion property IsSynchronous

		#region property UserID

		[DataMember]
		public int? UserID {
			get { return m_oUserID.Value; } // get
			set { m_oUserID.Value = value; } // set
		} // UserID

		private readonly SafeValue<int?> m_oUserID;

		#endregion property UserID

		#region property CustomerID

		[DataMember]
		public int? CustomerID {
			get { return m_oCustomerID.Value; } // get
			set { m_oCustomerID.Value = value; } // set
		} // CustomerID

		private readonly SafeValue<int?> m_oCustomerID;

		#endregion property CustomerID

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
						new QueryParameter("@Comment", Comment),
						new QueryParameter("@UserID", UserID.HasValue ? UserID.Value : (int?)null),
						new QueryParameter("@CustomerID", CustomerID.HasValue ? CustomerID.Value : (int?)null)
					);
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

		private ActionMetaData(ActionStatus nStatus) {
			m_oLockSave = new object();

			m_oStatus = new SafeValue<ActionStatus>(nStatus);
			m_oComment = new SafeValue<string>();
			m_oUserID = new SafeValue<int?>();
			m_oCustomerID = new SafeValue<int?>();
		} // constructor

		#endregion constructor

		#region fields

		private readonly object m_oLockSave;

		private AConnection m_oDB;
		private ASafeLog m_oLog;
		private int m_nServiceInstanceID;

		#endregion fields

		#endregion private
	} // struct ActionMetaData

	#endregion class ActionMetaData
} // namespace EzService
