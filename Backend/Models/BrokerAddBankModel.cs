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

		public override string ToString() {
		    return string.Format("{0}, {1}, {2}, {3}", BrokerEmail, AccountNumber, SortCode, BankAccountType);
	    }
    }
}
