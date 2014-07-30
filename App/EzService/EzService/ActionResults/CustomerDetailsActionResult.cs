namespace EzService {
	using System.Runtime.Serialization;
	using Ezbob.Backend.Models;

	[DataContract]
	public class CustomerDetailsActionResult : ActionResult {
		[DataMember]
		public CustomerDetails Value { get; set; }
	} // class CustomerDetailsActionResult
} // namespace EzService
