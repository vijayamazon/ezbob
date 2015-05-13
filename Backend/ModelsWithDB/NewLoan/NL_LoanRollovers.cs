namespace Ezbob.Backend.ModelsWithDB.NewLoan {
    using System;
    using System.Runtime.Serialization;
    using Ezbob.Utils;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
    public class NL_LoanRollovers {
        [PK]
        [NonTraversable]
        [DataMember]
        public int LoanRolloverID { get; set; }

        [FK("NL_LoanHistory", "LoanHistoryID")]
        [DataMember]
        public int LoanHistoryID { get; set; }

        [FK("Security_User", "UserId")]
        [DataMember]
        public int CreatedByUserID { get; set; }

        [FK("Security_User", "UserId")]
        [DataMember]
        public int? DeletedByUserID { get; set; }

        [FK("NL_LoanFees", "LoanFeeID")]
        [DataMember]
        public int? LoanFeeID { get; set; }

        [DataMember]
        public decimal FeeAmount { get; set; }

        [DataMember]
        public DateTime CreationTime { get; set; }

        [DataMember]
        public DateTime ExpirationTime { get; set; }

        [DataMember]
        public DateTime? CustomerActionTime { get; set; }

        [DataMember]
        public bool? IsAccepted { get; set; }

        [DataMember]
        public DateTime? DeletionTime { get; set; }
    }//class NL_LoanRollovers
}//ns
