namespace EzService.ActionResults {
	using System.Runtime.Serialization;

	[DataContract]
	public class UserSignupActionResult : ActionResult {
		[DataMember]
		public string Status { get; set; }

		[DataMember]
		public int UserID { get; set; }
	} // class UserSignupActionResult
} // namespace EzService
