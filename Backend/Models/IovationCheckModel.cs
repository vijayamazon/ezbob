namespace Ezbob.Backend.Models {
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    [DataContract(IsReference = true)]
    public class IovationCheckModel {
        [DataMember]
        public int CustomerID { get; set; }
        [DataMember]
        public string AccountCode { get; set; }
        [DataMember]
        public string BeginBlackBox { get; set; }
        [DataMember]
        public string EndUserIp { get; set; }
        [DataMember]
        public string Type { get; set; }
        [DataMember]
        public string Origin { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public string MobilePhoneNumber { get; set; }
        [DataMember]
        public bool mobilePhoneVerified { get; set; }
        [DataMember]
        public bool mobilePhoneSmsEnabled { get; set; }
        
        [DataMember]
        public IovationCheckMoreDataModel MoreData { get; set; }

        public override string ToString() {
            return "IovationCheckModel";
        }
    }

    [Serializable]
    [DataContract(IsReference = true)]
    public class IovationCheckMoreDataModel {
        [DataMember]
        public bool EmailVerified { get; set; }
        [DataMember]
        public string HomePhoneNumber { get; set; }
        [DataMember]
        public string BillingStreet { get; set; }
        [DataMember]
        public string BillingCity { get; set; }
        [DataMember]
        public string BillingCountry { get; set; }
        [DataMember]
        public string BillingPostalCode { get; set; }
        [DataMember]
        public string FirstName { get; set; }
        [DataMember]
        public string LastName { get; set; }
    }
}