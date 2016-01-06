namespace Ezbob.Backend.ModelsWithDB.OpenPlatform {
	using System;
	using System.Runtime.Serialization;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
	public class I_InvestorBankAccount {
        //[PK(true)]
        [DataMember]
		public int InvestorBankAccountID { get; set; }

		[FK("I_Investor", "InvestorID")]
        [DataMember]
        public int InvestorID { get; set; }

		[FK("I_InvestorAccountType", "InvestorAccountTypeID")]
		[DataMember]
		public int InvestorAccountTypeID { get; set; }
		
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
		[FK("Security_User", "UserId")]
		public int? UserID { get; set; }

		[DataMember]
		public DateTime Timestamp { get; set; }

		//////////////////////////////////////////

		[DataMember]
		[NonTraversable]
		public I_InvestorAccountType AccountType { get; set; }
	}//class I_InvestorBankAccount
}//ns
