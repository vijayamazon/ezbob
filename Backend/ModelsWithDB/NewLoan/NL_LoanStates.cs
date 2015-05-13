namespace Ezbob.Backend.ModelsWithDB.NewLoan {
    using System;
    using System.Runtime.Serialization;
    using Ezbob.Utils;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
    public class NL_LoanStates {
        [PK]
        [NonTraversable]
        [DataMember]
        public int LoanStateID { get; set; }

        [FK("NL_Loans", "LoanID")]
        [DataMember]
        public int LoanID { get; set; }

        [DataMember]
        public DateTime InsertDate { get; set; }

        [DataMember]
        public decimal OutstandingPrincipal { get; set; }

        [DataMember]
        public decimal OutstandingInterest { get; set; }

        [DataMember]
        public decimal OutstandingFee { get; set; }

        [DataMember]
        public decimal PaidPrincipal { get; set; }

        [DataMember]
        public decimal PaidInterest { get; set; }

        [DataMember]
        public decimal PaidFee { get; set; }

        [DataMember]
        public decimal Balance { get; set; }

        [DataMember]
        public int LateDays { get; set; }

        [Length(LengthType.MAX)]
        [DataMember]
        public string Notes { get; set; }
    }//class NL_LoanStates
}//ns
