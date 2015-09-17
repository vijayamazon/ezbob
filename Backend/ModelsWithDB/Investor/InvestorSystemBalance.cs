namespace Ezbob.Backend.ModelsWithDB.Investor
{
    using System;
    using System.Runtime.Serialization;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
    public class InvestorSystemBalance
    {
        [PK(true)]
        [DataMember]
        public int ID { get; set; }

        [DataMember]
        public InvestorType InvestorBankAccountID { get; set; }

        [DataMember]
        public String PreviousBalance { get; set; }

        [DataMember]
        public String NewBalance { get; set; }

        [DataMember]
        public TimeSpan TimeSpan { get; set; }

    }//class InvestorSystemBalance
}//ns