namespace EzService {
	using System.Collections.Generic;
	using System.Runtime.Serialization;

	#region class CrmLookupsActionResult

	[DataContract]
	public class CrmLookupsActionResult : ActionResult {
		[DataMember]
		public SortedDictionary<int, string> Actions { get; set; } // Actions

		[DataMember]
		public SortedDictionary<int, string> Statuses { get; set; } // Statuses
	} // class CrmLookupsActionResult

	#endregion class CrmLookupsActionResult
} // namespace EzService
