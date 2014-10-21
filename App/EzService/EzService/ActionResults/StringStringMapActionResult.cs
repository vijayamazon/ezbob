namespace EzService {
	using System.Collections.Generic;
	using System.Runtime.Serialization;

	[DataContract]
	public class StringStringMapActionResult : ActionResult {
		[DataMember]
		public SortedDictionary<string, string> Map { get; set; }
	} // class StringStringMapActionResult
} // namespace
