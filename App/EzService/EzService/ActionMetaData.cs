namespace EzService {
	using System;
	using System.Runtime.Serialization;
	using System.Threading;
	using Ezbob.Backend.Strategies;
	using Ezbob.Database;
	using Ezbob.Logger;

	public enum TriState {
		Unknown,
		Yes,
		No
	} // TriState

	[DataContract(IsReference = true)]
	public class ActionMetaData {

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
				CustomerID = nCustomerID,
			};

			amd.Save();

			return amd;
		} // Create

		[DataMember]
		public Guid ActionID { get; private set; }

		[DataMember]
		public string Name { get; private set; }

		[DataMember]
		public ActionStatus Status {
			get { return m_oStatus.Value; } // get
			set { m_oStatus.Value = value; } // set
		} // Status

		private readonly SafeValue<ActionStatus> m_oStatus;

		[DataMember]
		public string Comment {
			get { return m_oComment.Value; } // get
			set { m_oComment.Value = value; } // set
		} // Status

		private readonly SafeValue<string> m_oComment;

		[DataMember]
		public bool IsSynchronous { get; private set; }

		[DataMember]
		public int? UserID {
			get { return m_oUserID.Value; } // get
			set { m_oUserID.Value = value; } // set
		} // UserID

		private readonly SafeValue<int?> m_oUserID;

		[DataMember]
		public int? CustomerID {
			get { return m_oCustomerID.Value; } // get
			set { m_oCustomerID.Value = value; } // set
		} // CustomerID

		private readonly SafeValue<int?> m_oCustomerID;

		public AStrategy Strategy {
			get {
				AStrategy stra;

				lock (this.strategyLock)
					stra = this.strategy;

				return stra;
			} // get
			set {
				lock (this.strategyLock)
					this.strategy = value;
			} // set
		} // Strategy

		private readonly object strategyLock;
		private AStrategy strategy;

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

		public Thread UnderlyingThread {
			get { return m_oUnderlyingThread; } // get
			set {
				if (ReferenceEquals(value, null))
					throw new ArgumentNullException("value", "UnderlyingThread cannot be null");

				m_oUnderlyingThread = value;
			} // set
		} // UnderlyingThread

		private Thread m_oUnderlyingThread;

		public void Save(ActionStatus nNewStatus) {
			Status = nNewStatus;
			Save();
		} // Save

		public void Save() {
			lock (m_oLockSave) {
				try {
					m_oDB.ExecuteNonQuery("EzServiceSaveActionMetaData",
						CommandSpecies.StoredProcedure,
						new QueryParameter("@Now", DateTime.UtcNow),
						new QueryParameter("@InstanceID", m_nServiceInstanceID),
						new QueryParameter("@ActionName", Name),
						new QueryParameter("@ActionID", ActionID),
						new QueryParameter("@IsSync", IsSynchronous),
						new QueryParameter("@Status", (int)Status),
						new QueryParameter("@CurrentThreadID", Thread.CurrentThread.ManagedThreadId),
						new QueryParameter("@UnderlyingThreadID", UnderlyingThread.ManagedThreadId),
						new QueryParameter("@Comment", Comment),
						new QueryParameter("@UserID", UserID),
						new QueryParameter("@CustomerID", CustomerID)
					);
				}
				catch (Exception e) {
					m_oLog.Alert(e, "Failed to save action status of {0} to DB.", this);
				} // try
			} // lock
		} // Save

		private ActionMetaData(ActionStatus nStatus) {
			m_oLockSave = new object();

			m_oStatus = new SafeValue<ActionStatus>(nStatus);
			m_oComment = new SafeValue<string>();
			m_oUserID = new SafeValue<int?>();
			m_oCustomerID = new SafeValue<int?>();

			this.strategyLock = new object();
			Strategy = null;
		} // constructor

		private readonly object m_oLockSave;

		private AConnection m_oDB;
		private ASafeLog m_oLog;
		private int m_nServiceInstanceID;
	} // struct ActionMetaData
} // namespace EzService
