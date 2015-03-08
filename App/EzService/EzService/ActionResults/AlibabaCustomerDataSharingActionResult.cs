namespace EzService {
	using System.Runtime.Serialization;
	using Ezbob.Backend.Models.Alibaba;

	[DataContract]
	public class AlibabaCustomerDataSharingActionResult : ActionResult {
		
		[DataMember]
		public CustomerDataSharing Result { get; set; }
	} 

} // namespace
