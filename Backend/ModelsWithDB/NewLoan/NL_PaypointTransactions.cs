namespace Ezbob.Backend.ModelsWithDB.NewLoan {
    using System;
    using System.Runtime.Serialization;
    using Ezbob.Utils;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
    public class NL_PaypointTransactions {
        [PK]
        [NonTraversable]
        [DataMember]
        public int PaypointTransactionID { get; set; }

        [FK("NL_Payments", "PaymentID")]
        [DataMember]
        public int? PaymentID { get; set; }

        [DataMember]
        public DateTime TransactionTime { get; set; }

        [DataMember]
        public decimal Amount { get; set; }

        [Length(LengthType.MAX)]
        [DataMember]
        public string Notes { get; set; }

        [FK("NL_PaypointTransactionStatuses", "PaypointTransactionStatusID")]
        [DataMember]
        public int? PaypointTransactionStatusID { get; set; }

        [Length(100)]
        [DataMember]
        public string PaypointUniqID { get; set; }

        [FK("PayPointCard", "Id")]
        [DataMember]
        public int PaypointCardID { get; set; }

        [Length(10)]
        [DataMember]
        public string IP { get; set; }
    }//class NL_PaypointTransactions
}//ns
