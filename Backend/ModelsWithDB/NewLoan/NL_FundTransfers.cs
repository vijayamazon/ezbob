namespace Ezbob.Backend.ModelsWithDB.NewLoan {
    using System;
    using System.Runtime.Serialization;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
    public class NL_FundTransfers {
		[PK(true)]
        [DataMember]
        public int FundTransferID { get; set; }

        [FK("NL_Loans", "LoanID")]
        [DataMember]
        public int LoanID { get; set; }

        [DataMember]
        public decimal Amount { get; set; }

        [DataMember]
        public DateTime TransferTime { get; set; }

        [DataMember]
        public bool IsActive { get; set; }

		[FK("LoanTransactionMethod", "Id")]
		[DataMember]
		public int LoanTransactionMethodID { get; set; }

		//[DataMember]
		//public NL_PacnetTransactions PacnetTransaction { get; set; }

    }//class NL_FundTransfers
}//ns
