namespace EzBobModels.Broker
{
    using System;

    public class Broker
    {
        public string FirmName { get; set; }//Mandatory
        public string FirmRegNum { get; set; }
        public string ContactName { get; set; }//Mandatory
        public string ContactEmail { get; set; }//Mandatory
        public string ContactMobile { get; set; }//Mandatory
        public string ContactOtherPhone { get; set; }
        public string SourceRef { get; set; }//Mandatory
        public decimal EstimatedMonthlyClientAmount { get; set; }
        public string Password { get; set; }//Mandatory
        public int BrokerID { get; set; }
        public string FirmWebSiteUrl { get; set; }
        public int EstimatedMonthlyApplicationCount { get; set; }
        public DateTime AgreedToTermsDate { get; set; }
        public DateTime AgreedToPrivacyPolicyDate { get; set; }
        public int? BrokerTermsID { get; set; }
        public bool IsTest { get; set; }
        public string ReferredBy { get; set; }
        public int? OldBrokerId { get; set; }
        public int? WhiteLabelId { get; set; }
        public string LicenseNumber { get; set; }
        public bool? FCARegistered { get; set; }
        public int OriginID { get; set; }
    }
}
