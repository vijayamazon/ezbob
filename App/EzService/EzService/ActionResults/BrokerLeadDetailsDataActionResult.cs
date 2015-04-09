namespace EzService {
	using System.Runtime.Serialization;
	using Ezbob.Backend.Models;

    [DataContract]
    public class BrokerLeadDetailsDataActionResult : ActionResult {
        [DataMember]
        public BrokerLeadDataModel BrokerLeadDataModel { get; set; }
    } // class BrokerLeadDetailsDataActionResult

} // namespace EzService
