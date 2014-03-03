using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EzService {
	#region class StringListActionResult

	[DataContract]
	public class StringListActionResult : ActionResult {
		[DataMember]
		public List<string> Records { get; set; }
	} // struct ActionResult

	#endregion class StringListActionResult
} // namespace EzService
