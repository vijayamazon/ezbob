namespace EzService {
	using System.Runtime.Serialization;
	using Ezbob.Backend.Models;

	[DataContract]
	public class AccountsToUpdateActionResult : ActionResult {
		[DataMember]
		public AccountsToUpdate AccountInfo { get; set; }
	} // class AccountsToUpdateActionResult
} // namespace
