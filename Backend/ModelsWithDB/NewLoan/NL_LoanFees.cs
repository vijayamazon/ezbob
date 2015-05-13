namespace Ezbob.Backend.ModelsWithDB.NewLoan {
    using System;
    using System.Runtime.Serialization;
    using Ezbob.Utils;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
    public class NL_LoanFees {
        [PK]
        [NonTraversable]
        [DataMember]
        public int LoanFeeID { get; set; }

        [FK("NL_LoanFeeTypes", "LoanFeeTypeID")]
        [DataMember]
        public int? LoanFeeTypeID { get; set; }

        [FK("NL_Loans", "LoanID")]
        [DataMember]
        public int? LoanID { get; set; }

        [FK("Security_User", "UserId")]
        [DataMember]
        public int AssignedByUserID { get; set; }

        [DataMember]
        public decimal Amount { get; set; }

        [DataMember]
        public DateTime CreatedTime { get; set; }

        [DataMember]
        public DateTime AssignTime { get; set; }

        [FK("Security_User", "UserId")]
        [DataMember]
        public int DeletedByUserID { get; set; }

        [DataMember]
        public DateTime DisabledTime { get; set; }

        [Length(LengthType.MAX)]
        [DataMember]
        public string Notes { get; set; }
    }//class NL_LoanFees
}//ns
