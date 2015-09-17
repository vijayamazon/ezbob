namespace Ezbob.Backend.ModelsWithDB.Investor {
    using System;
    using System.Runtime.Serialization;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
    public class InvestorBankAccount
    {
		[PK(true)]
		[DataMember]
        public int ID { get; set; }

        [DataMember]
        public int InvestorID { get; set; }

		[DataMember]
        public String BankName { get; set; }

        [DataMember]
        public String BankCode { get; set; }

        [DataMember]
        public String BankCountryID { get; set; }

        [DataMember]
        public String BankCountryName { get; set; }

        [DataMember]
        public String BankBranchNumber{ get; set; }

        [DataMember]
        public String BankBranchName { get; set; }

        [DataMember]
        public String BankAccountNumber { get; set; }

        [DataMember]
        public String BankAccountName { get; set; }

        [DataMember]
        public bool IsActive { get; set; }

        [DataMember]
        public TimeSpan TimeSpan { get; set; }

    }//class InvestorBankAccount
}//ns