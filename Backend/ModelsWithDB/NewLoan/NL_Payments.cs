namespace Ezbob.Backend.ModelsWithDB.NewLoan {
    using System;
    using System.Runtime.Serialization;
    using Ezbob.Utils;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
    public class NL_Payments {
        [PK]
        [NonTraversable]
        [DataMember]
        public int PaymentID { get; set; }

        [FK("LoanTransactionMethod", "Id")]
        [DataMember]
        public int PaymentMethodID { get; set; }

        [FK("NL_PaymentStatuses", "PaymentStatusID")]
        [DataMember]
        public int PaymentStatusID { get; set; }

        [DataMember]
        public DateTime PaymentTime { get; set; }

        [DataMember]
        public bool IsActive { get; set; }

        [DataMember]
        public DateTime CreationTime { get; set; }

        [FK("Security_User", "UserId")]
        [DataMember]
        public int? CreatedByUserID { get; set; }

        [DataMember]
        public DateTime DeletionTime { get; set; }

        [FK("Security_User", "UserId")]
        [DataMember]
        public int? DeletedByUserID { get; set; }

        [Length(LengthType.MAX)]
        [DataMember]
        public string Notes { get; set; }
    }//class NL_Payments
}//ns
