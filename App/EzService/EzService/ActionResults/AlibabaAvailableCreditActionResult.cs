namespace EzService {
	using System.Runtime.Serialization;
	using Ezbob.Backend.Models.ExternalAPI;

	[DataContract]
	public class AlibabaAvailableCreditActionResult : ActionResult {
		[DataMember]
		public AlibabaAvailableCreditResult Result { get; set; }
	} // class LotteryActionResult
} // namespace EzService
