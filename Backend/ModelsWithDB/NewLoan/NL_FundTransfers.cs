namespace Ezbob.Backend.ModelsWithDB.NewLoan {
    using System;
    using System.Runtime.Serialization;
    using Ezbob.Utils;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
    public class NL_FundTransfers {
        [PK]
        [NonTraversable]
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
    }//class NL_FundTransfers
}//ns
