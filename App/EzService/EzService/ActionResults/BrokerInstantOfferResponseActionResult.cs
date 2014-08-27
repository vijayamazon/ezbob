namespace EzService.ActionResults {
	using System.Runtime.Serialization;
	using Ezbob.Backend.ModelsWithDB;

	[DataContract]
	public class BrokerInstantOfferResponseActionResult: ActionResult {
		[DataMember]
		public BrokerInstantOfferResponse Response { get; set; }
	}
}
