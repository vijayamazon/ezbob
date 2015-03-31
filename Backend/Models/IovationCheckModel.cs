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
        
        public override string ToString() {
            return "IovationCheckModel";
        }
    }
}