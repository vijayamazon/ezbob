namespace Ezbob.Backend.ModelsWithDB.NewLoan {
    using System;
    using System.Runtime.Serialization;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
    public class NL_LoanHistory {
        [PK(true)]
        [DataMember]
        public int LoanHistoryID { get; set; }

        [DataMember]
        public DateTime EventTime { get; set; }

        [Length(LengthType.MAX)]
        [DataMember]
        public string Description { get; set; }

        [FK("NL_Loans", "LoanID")]
        [DataMember]
        public int? LoanID { get; set; }

        [FK("Security_User", "UserId")]
        [DataMember]
        public int? UserID { get; set; }

        [FK("NL_LoanLegals", "LoanLegalID")]
        [DataMember]
        public int? LoanLegalID { get; set; }
    }//class NL_LoanHistory
}//ns
