namespace Ezbob.Backend.Models  {
    using System.Runtime.Serialization;

    [DataContract]
	public class CompanyInfo {
        [DataMember]
		public string LegalStatus { get; set; }
        [DataMember]
		public string BusinessStatus { get; set; }
        [DataMember]
		public string MatchScore { get; set; }
        [DataMember]
		public string BusRefNum { get; set; }
        [DataMember]
		public string BusName { get; set; }
        [DataMember]
		public string AddrLine1 { get; set; }
        [DataMember]
		public string AddrLine2 { get; set; }
        [DataMember]
		public string AddrLine3 { get; set; }
        [DataMember]
		public string AddrLine4 { get; set; }
        [DataMember]
		public string PostCode { get; set; }
        [DataMember]
		public string SicCodeType { get; set; }
        [DataMember]
		public string SicCode { get; set; }
        [DataMember]
		public string SicCodeDesc { get; set; }
        [DataMember]
		public string MatchedBusName { get; set; }
        [DataMember]
		public string MatchedBusNameType { get; set; }
	} // class CompanyInfo
} // namespace
