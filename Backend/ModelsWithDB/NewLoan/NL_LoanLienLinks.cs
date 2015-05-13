namespace Ezbob.Backend.ModelsWithDB.NewLoan {
    using System.Runtime.Serialization;
    using Ezbob.Utils;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
    public class NL_LoanLienLinks {
        [PK]
        [NonTraversable]
        [DataMember]
        public int LoanLienLinkID { get; set; }

        [FK("NL_Loans", "LoanID")]
        [DataMember]
        public int LoanID { get; set; }

        [FK("LoanLien", "Id")]
        [DataMember]
        public int LoanLienID { get; set; }

        [DataMember]
        public decimal Amount { get; set; }
    }//class NL_LoanLienLinks
}//ns
