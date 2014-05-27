namespace EzService {
	using System.Runtime.Serialization;

	#region class CrmLookupsActionResult

	[DataContract]
	public class CrmLookupsActionResult : ActionResult {
		[DataMember]
		public IdNameModel[] Actions { get; set; } // Actions

		[DataMember]
		public IdNameModel[] Statuses { get; set; } // Statuses

		[DataMember]
		public IdNameModel[] Ranks { get; set; } // Ranks
	} // class CrmLookupsActionResult

	#endregion class CrmLookupsActionResult
} // namespace EzService
