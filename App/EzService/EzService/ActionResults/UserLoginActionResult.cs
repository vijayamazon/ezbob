namespace EzService {
	using System.Runtime.Serialization;

	[DataContract]
	public class UserLoginActionResult : ActionResult {
		[DataMember]
		public string Status { get; set; }

		[DataMember]
		public int SessionID { get; set; }

		[DataMember]
		public string ErrorMessage { get; set; }

		[DataMember]
		public string RefNumber { get; set; }
	} // class UserLoginActionResult
} // namespace EzService
