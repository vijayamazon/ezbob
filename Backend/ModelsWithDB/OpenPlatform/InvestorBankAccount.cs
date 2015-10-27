namespace Ezbob.Backend.ModelsWithDB.OpenPlatform {
	using System;
	using System.Runtime.Serialization;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
	public class InvestorBankAccount {
        [PK(true)]
        [DataMember]
		public int InvestorBankAccountID { get; set; }
		
		[FK("Investor", "InvestorID")]
        [DataMember]
        public int InvestorID { get; set; }
		
		[Length(255)]
		[DataMember]
		public string BankName { get; set; }

		[Length(255)]
		[DataMember]
		public string BankCode { get; set; }
		
		[Length(255)]
		[DataMember]
		public string BankCountryID { get; set; }
		
		[Length(255)]
		[DataMember]
		public string BankBranchName { get; set; }
		
		[Length(255)]
		[DataMember]
		public string BankBranchNumber { get; set; }
		
		[Length(255)]
		[DataMember]
		public string BankAccountName { get; set; }
		
		[Length(255)]
		[DataMember]
		public string BankAccountNumber { get; set; }
		
		[Length(255)]
		[DataMember]
		public string RepaymentKey { get; set; }

		[DataMember]
		public bool IsActive { get; set; }

		[DataMember]
		public DateTime Timestamp { get; set; }
	}//class InvestorBankAccount
}//ns
