namespace EzService {
	using System.Runtime.Serialization;
	using Ezbob.Backend.Models;

	[DataContract]
	public class CrmLookupsActionResult : ActionResult {
		[DataMember]
		public IdNameModel[] Actions { get; set; } // Actions

		[DataMember]
		public CrmStatusGroup[] Statuses { get; set; } // Statuses

		[DataMember]
		public IdNameModel[] Ranks { get; set; } // Ranks
	} // class CrmLookupsActionResult
} // namespace EzService
