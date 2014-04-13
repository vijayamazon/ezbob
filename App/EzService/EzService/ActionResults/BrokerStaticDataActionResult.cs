namespace EzService {
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using Ezbob.Backend.Models;

	[DataContract]
	public class BrokerStaticDataActionResult : ActionResult {
		[DataMember]
		public FileDescription[] Files { get; set; }

		[DataMember]
		public SortedDictionary<int, string> Actions { get; set; } // Actions

		[DataMember]
		public SortedDictionary<int, string> Statuses { get; set; } // Statuses

		[DataMember]
		public int MaxPerNumber { get; set; }

		[DataMember]
		public int MaxPerPage { get; set; }

		[DataMember]
		public int TermsID { get; set; }

		[DataMember]
		public string Terms { get; set; }
	} // class BrokerStaticDataActionResult
} // namespace EzService
