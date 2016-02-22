namespace EzService {
	using System.Runtime.Serialization;

	[DataContract]
	public class SetPasswordActionResult : ActionResult {
		[DataMember]
		public string ErrorMsg { get; set; }

		[DataMember]
		public int UserID { get; set; }

		[DataMember]
		public bool IsBroker { get; set; }

		[DataMember]
		public string Email { get; set; }

		[DataMember]
		public int SessionID { get; set; }

		[DataMember]
		public bool IsDisabled { get; set; }
	} // class SetPasswordActionResult
} // namespace
