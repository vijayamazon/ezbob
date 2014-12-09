namespace EzService {
	using System.Runtime.Serialization;

	[DataContract]
	public class BrokerLeadDetailsActionResult : ActionResult {
		[DataMember]
		public int LeadID { get; set; }

		[DataMember]
		public string LeadEmail { get; set; }

		[DataMember]
		public int CustomerID { get; set; }

		[DataMember]
		public string FirstName { get; set; }

		[DataMember]
		public string LastName { get; set; }
	} // class BrokerLeadDetailsActionResult

} // namespace EzService
