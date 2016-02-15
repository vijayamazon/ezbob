namespace EzService.ActionResults {
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using Ezbob.Backend.Strategies.LandRegistry;

	[DataContract]
	public class LandRegistryActionResult : ActionResult {
		[DataMember]
		public IList<LandRegistryDB> Value { get; set; }
	} // class LongActionResult
} // namespace
