namespace EzService {
	using System.Runtime.Serialization;
	using Ezbob.Backend.Models;

	[DataContract]
	public class BrokerStaticDataActionResult : ActionResult {
		[DataMember]
		public FileDescription[] Files { get; set; }

		[DataMember]
		public int MaxPerNumber { get; set; }

		[DataMember]
		public int MaxPerPage { get; set; }

		[DataMember]
		public int TermsID { get; set; }

		[DataMember]
		public string Terms { get; set; }

		[DataMember]
		public CrmStaticModel Crm { get; set; }
	} // class BrokerStaticDataActionResult
} // namespace EzService
