namespace EzService {
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using Ezbob.Backend.Models;

	[DataContract]
	public class MessagesListActionResult : ActionResult {
		[DataMember]
		public List<MessagesModel> Messages { get; set; }
	} // class MessagesListActionResult
} // namespace EzService
