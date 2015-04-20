namespace Ezbob.Backend.Models
{
    using System.Runtime.Serialization;

    [DataContract]
    public class ExperianTargetingRequest
    {
        [DataMember]
        public int CustomerID { get; set; }
        [DataMember]
        public string Postcode { get; set; }
        [DataMember]
        public string CompanyName { get; set; }
        [DataMember]
        public string Filter { get; set; }
        [DataMember]
        public string RefNum { get; set; }
    }
}
