namespace EzService {
	using System.Runtime.Serialization;
	using Ezbob.Backend.ModelsWithDB;

	[DataContract]
	public class PricingModelModelActionResult : ActionResult {
		[DataMember]
		public PricingModelModel Value { get; set; }
	} // class PricingModelModelActionResult
} // namespace EzService
