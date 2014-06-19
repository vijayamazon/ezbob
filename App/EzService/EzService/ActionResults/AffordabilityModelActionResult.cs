namespace EzService {
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using Ezbob.Backend.Models;

	#region class AffordabilityModelActionResult

	[DataContract]
	public class AffordabilityModelActionResult : ActionResult
	{
		[DataMember]
		public List<AffordabilityModel> Value { get; set; }
	} // class AffordabilityModelActionResult

	#endregion class AffordabilityModelActionResult
} // namespace EzService
