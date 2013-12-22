using System;
using System.Runtime.Serialization;
using System.Threading;

namespace EzService {
	#region enum TriState

	public enum TriState {
		Unknown,
		Yes,
		No
	} // TriState

	#endregion enum TriState

	#region enum ActionStatus

	[DataContract]
	public enum ActionStatus {
		[EnumMember]
		InProgress,

		/// <summary>
		/// Action completed successfully.
		/// </summary>
		[EnumMember]
		Done,

		/// <summary>
		/// Action completed, but result is "failure".
		/// </summary>
		[EnumMember]
		Finished,

		/// <summary>
		/// Action failed to complete.
		/// </summary>
		[EnumMember]
		Failed,

		/// <summary>
		/// Action was terminated.
		/// </summary>
		[EnumMember]
		Terminated,

		[EnumMember]
		Unknown,
	} // enum ActionStatus

	#endregion enum ActionStatus

	#region class ActionMetaData

	[DataContract]
	public class ActionMetaData {
		#region public

		#region static methods

		#region method NewAsync

		public static ActionMetaData NewAsync(ActionStatus status = ActionStatus.InProgress, string comment = null) {
			return Create(false, status, comment);
		} // NewAsync

		#endregion method NewAsync

		#region method NewSync

		public static ActionMetaData NewSync(ActionStatus status = ActionStatus.InProgress, string comment = null) {
			return Create(true, status, comment);
		} // NewSync

		#endregion method NewSync

		#endregion static methods

		#region serialised properties

		[DataMember]
		public Guid ActionID { get; private set; }

		[DataMember]
		public ActionStatus Status;

		[DataMember]
		public string Comment;

		[DataMember]
		public bool IsSynchronous { get; private set; }

		#endregion serialised properties

		#region method ToString

		public override string ToString() {
			return string.Format(
				"{{ {0}: [{1}sync {2}] {3}: {4} }}",
				ActionID,
				IsSynchronous ? "" : "a",
				UnderlyingThread.ManagedThreadId,
				Status,
				Comment ?? "no comments"
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
				if (value == null)
					throw new ArgumentNullException("UnderlyingThread");

				m_oUnderlyingThread = value;
			} // set
		} // UnderlyingThread

		private Thread m_oUnderlyingThread;

		#endregion property UnderlyingThread

		#endregion public

		#region private

		#region method Create

		private static ActionMetaData Create(bool bIsSynchronous, ActionStatus nStatus, string sComment) {
			Guid oActionID = Guid.NewGuid();

			while (oActionID == Guid.Empty)
				oActionID = Guid.NewGuid();

			return new ActionMetaData {
				ActionID = oActionID,
				IsSynchronous = bIsSynchronous,
				Status = nStatus,
				UnderlyingThread = Thread.CurrentThread,
				Comment = sComment
			};
		} // Create

		#endregion method Create

		#endregion private
	} // struct ActionMetaData

	#endregion class ActionMetaData
} // namespace EzService
