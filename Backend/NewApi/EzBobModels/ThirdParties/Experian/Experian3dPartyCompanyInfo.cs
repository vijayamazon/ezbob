namespace EzBobModels.ThirdParties.Experian
{
    public class Experian3dPartyCompanyInfo
    {
        public string LegalStatus { get; set; }
        public string BusinessStatus { get; set; }
        public string MatchScore { get; set; }
        public string BusRefNum { get; set; }
        public string BusName { get; set; }
        public string AddrLine1 { get; set; }
        public string AddrLine2 { get; set; }
        public string AddrLine3 { get; set; }
        public string AddrLine4 { get; set; }
        public string PostCode { get; set; }
        public string SicCodeType { get; set; }
        public string SicCode { get; set; }
        public string SicCodeDesc { get; set; }
        public string MatchedBusName { get; set; }
        public string MatchedBusNameType { get; set; }
    }
}
