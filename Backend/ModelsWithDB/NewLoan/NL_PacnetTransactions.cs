namespace Ezbob.Backend.ModelsWithDB.NewLoan {
    using System;
    using System.Runtime.Serialization;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
    public class NL_PacnetTransactions {
		[PK(true)]
        [DataMember]
        public int PacnetTransactionID { get; set; }

        [FK("NL_FundTransfers", "FundTransferID")]
        [DataMember]
        public int? FundTransferID { get; set; }

        [DataMember]
        public DateTime? TransactionTime { get; set; }

        [DataMember]
        public decimal Amount { get; set; }

        [Length(LengthType.MAX)]
        [DataMember]
        public string Notes { get; set; }

        [FK("NL_PacnetTransactionStatuses", "PacnetTransactionStatusID")]
        [DataMember]
        public int? PacnetTransactionStatusID { get; set; }

        [Length(100)]
        [DataMember]
        public string TrackingNumber { get; set; }

        
        [DataMember]
        public DateTime StatusUpdatedTime { get; set; }


		public string TransactionStatus { get; set; }
        
    }//class NL_PacnetTransactions
}//ns
