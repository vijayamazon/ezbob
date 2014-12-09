namespace EzService {
	using System.Collections.Generic;
	using Ezbob.Backend.Models;
	using System.Runtime.Serialization;

	[DataContract]
	public class BrokerCustomerFilesActionResult : ActionResult {
		[DataMember]
		public List<BrokerCustomerFile> Files { get; set; }
	} // class BrokerCustomerFilesActionResult

} // namespace EzService
