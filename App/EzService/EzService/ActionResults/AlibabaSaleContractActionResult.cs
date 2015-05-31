namespace EzService {
	using System.Runtime.Serialization;
	using Ezbob.Backend.Models.ExternalAPI;

	[DataContract]
    public class AlibabaSaleContractActionResult : ActionResult
    {
		[DataMember]
        public AlibabaSaleContractResult Result { get; set; }
	} // class LotteryActionResult
} // namespace EzService
