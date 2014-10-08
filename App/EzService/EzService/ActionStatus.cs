namespace EzService {
	using System.Runtime.Serialization;

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
} // namespace
