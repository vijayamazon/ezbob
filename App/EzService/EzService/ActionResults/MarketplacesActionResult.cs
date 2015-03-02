namespace EzService {
	using System.Runtime.Serialization;
	using Ezbob.Backend.Models;

	[DataContract]
	public class MarketplacesActionResult : ActionResult {
		[DataMember]
		public MpModel MpModel { get; set; }
	} // class MarketplacesActionResult
} // namespace EzService
