﻿namespace Ezbob.Backend.ModelsWithDB.OpenPlatform {
	using System;
	using System.Runtime.Serialization;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
	public class InvestorBankAccountTransaction {
        [PK(true)]
        [DataMember]
		public int InvestorBankAccountTransactionID { get; set; }
		
		[FK("InvestorBankAccount", "InvestorBankAccountID")]
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
    }//class InvestorBankAccountTransaction
}//ns
