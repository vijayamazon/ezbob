namespace Ezbob.Backend.ModelsWithDB.Investor
{
    using System;
    using System.Runtime.Serialization;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
    public class InvestorBankAccountTransaction
    {
        [PK(true)]
        [DataMember]
        public int ID { get; set; }

        [DataMember]
        public int InvestorBankAccountID { get; set; }

        [DataMember]
        public int PreviousBalance { get; set; }

        [DataMember]
        public int NewBalance { get; set; }

        [DataMember]
        public int TransactionAmount { get; set; }

        [DataMember]
        public TimeSpan TimeSpan { get; set; }

    }//class InvestorBankAccountTransaction
}//ns