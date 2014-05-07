namespace EzService {
	using System.Runtime.Serialization;
	using EzBob.Backend.Strategies.PricingModel;

	#region class PricingModelModelActionResult

	[DataContract]
	public class PricingModelModelActionResult : ActionResult
	{
		[DataMember]
		public PricingModelModel Value { get; set; }
	} // class PricingModelModelActionResult

	#endregion class PricingModelModelActionResult
} // namespace EzService
