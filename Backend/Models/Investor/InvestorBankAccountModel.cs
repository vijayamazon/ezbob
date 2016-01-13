namespace Ezbob.Backend.Models.Investor {
	using System;
	using System.Runtime.Serialization;

	[DataContract(IsReference = true)]
	public class InvestorBankAccountModel {
		[DataMember]
		public int InvestorBankAccountID { get; set; }

		[DataMember]
		public int InvestorID { get; set; }

		[DataMember]
		public string BankName { get; set; }

		[DataMember]
		public string BankCode { get; set; }

		[DataMember]
		public string BankCountryID { get; set; }

		[DataMember]
		public string BankBranchName { get; set; }

		[DataMember]
		public string BankBranchNumber { get; set; }

		[DataMember]
		public string BankAccountName { get; set; }

		[DataMember]
		public string BankAccountNumber { get; set; }

		[DataMember]
		public string RepaymentKey { get; set; }

		[DataMember]
		public bool IsActive { get; set; }

		[DataMember]
		public DateTime Timestamp { get; set; }

		[DataMember]
		public InvestorAccountTypeModel AccountType { get; set; }
	}

	[DataContract(IsReference = true)]
	public class InvestorAccountTypeModel {
		[DataMember]
		public int InvestorAccountTypeID { get; set; }

		[DataMember]
		public string Name { get; set; }
	}
}
