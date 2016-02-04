namespace EzService {
	using System.Runtime.Serialization;
	using Ezbob.Backend.Strategies.PricingModel;

	[DataContract]
	public class PricingModelModelActionResult : ActionResult {
		[DataMember]
		public PricingModelModel Value { get; set; }
	} // class PricingModelModelActionResult
} // namespace EzService
