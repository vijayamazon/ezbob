namespace Ezbob.Backend.ModelsWithDB.NewLoan {
    using System.Runtime.Serialization;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
    public class NL_BlendedLoans {
        [PK(true)]
        [DataMember]
        public int BlendedLoanID { get; set; }

        [FK("NL_Loans", "LoanID")]
        [DataMember]
        public int LoanID { get; set; }
    }//class NL_BlendedLoans
}//ns
