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
	} // class BrokerStaticDataActionResult

	[DataContract]
	public class IdNameModel
	{
		[DataMember]
		public int Id { get; set; }
		[DataMember]
		public string Name { get; set; }
	}
} // namespace EzService
