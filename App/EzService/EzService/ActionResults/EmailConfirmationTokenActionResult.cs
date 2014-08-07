namespace EzService {
	using System;
	using System.Runtime.Serialization;

	[DataContract]
	public class EmailConfirmationTokenActionResult : ActionResult {
		[DataMember]
		public Guid Token { get; set; }

		[DataMember]
		public string Address { get; set; }
	} // class EmailConfirmationToken
} // namespace
