namespace Ezbob.Backend.ModelsWithDB.OpenPlatform {
	using System;
	using System.Runtime.Serialization;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
	public class I_InvestorBankAccountTransaction {
        [PK(true)]
        [DataMember]
		public int InvestorBankAccountTransactionID { get; set; }

		[FK("I_InvestorBankAccount", "InvestorBankAccountID")]
        [DataMember]
        public int InvestorBankAccountID { get; set; }
		
		[DataMember]
		public decimal? PreviousBalance { get; set; }

		[DataMember]
		public decimal? NewBalance { get; set; }

		[DataMember]
		public decimal? TransactionAmount { get; set; }

		[DataMember]
		public DateTime Timestamp { get; set; }

		[DataMember]
		[FK("Security_User", "UserId")]
		public int? UserID { get; set; }

		[DataMember]
		[Length(500)]
		public string Comment { get; set; }
	}//class I_InvestorBankAccountTransaction
}//ns
