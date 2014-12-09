using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EzService {

	[DataContract]
	public class StringListActionResult : ActionResult {
		[DataMember]
		public List<string> Records { get; set; }
	} // struct ActionResult

} // namespace EzService
