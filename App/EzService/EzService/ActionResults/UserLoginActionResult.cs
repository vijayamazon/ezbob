﻿namespace EzService {
	using System.Runtime.Serialization;

	[DataContract]
	public class UserLoginActionResult : ActionResult {
		[DataMember]
		public string Status { get; set; }

		[DataMember]
		public int SessionID { get; set; }
	} // class UserLoginActionResult
} // namespace EzService
