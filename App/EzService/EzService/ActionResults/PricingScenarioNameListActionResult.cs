namespace EzService.ActionResults {
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using Ezbob.Backend.Models;

	[DataContract]
	public class PricingScenarioNameListActionResult : ActionResult {
		[DataMember]
		public List<PricingScenarioName> Names { get; set; }
	} // class PricingScenarioNameListActionResult
} // namespace
