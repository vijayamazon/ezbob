namespace Ezbob.Backend.ModelsWithDB.NewLoan {
    using System;
    using System.Runtime.Serialization;
    using Ezbob.Utils;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
    public class NL_LoanSchedules {
        [PK]
        [NonTraversable]
        [DataMember]
        public int LoanScheduleID { get; set; }

        [FK("NL_LoanHistory", "LoanHistoryID")]
        [DataMember]
        public int LoanHistoryID { get; set; }

        [DataMember]
        public int Position { get; set; }

        [DataMember]
        public DateTime PlannedDate { get; set; }

        [DataMember]
        public DateTime? ClosedTime { get; set; }

        [DataMember]
        public decimal Principal { get; set; }

        [DataMember]
        public decimal InterestRate { get; set; }
    }//NL_LoanSchedules
}//ns
