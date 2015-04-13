namespace Ezbob.Backend.Models
{
    using System.Runtime.Serialization;

    [DataContract(IsReference = true)]
    public class BrokerAddBankModel
    {
        [DataMember]
        public string BrokerEmail { get; set; }
        [DataMember]
        public string AccountNumber { get; set; }
        [DataMember]
        public string SortCode { get; set; }
        [DataMember]
        public string BankAccountType { get; set; }
    }
}
