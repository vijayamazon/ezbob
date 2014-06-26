namespace EzService {
	using System.Runtime.Serialization;
	using Ezbob.Backend.Models;

	[DataContract]
	public class MarketplacesActionResult : ActionResult {
		[DataMember]
		public string Models { get; set; }

		[DataMember]
		public AffordabilityData[] Affordability { get; set; }
	} // class MarketplacesActionResult
} // namespace EzService
