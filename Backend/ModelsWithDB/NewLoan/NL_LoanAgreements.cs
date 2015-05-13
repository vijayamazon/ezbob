namespace Ezbob.Backend.ModelsWithDB.NewLoan {
    using System.Runtime.Serialization;
    using Ezbob.Utils;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
    public class NL_LoanAgreements {
        [PK]
        [NonTraversable]
        [DataMember]
        public int LoanAgreementID { get; set; }

        [FK("NL_LoanHistory", "LoanHistoryID")]
        [DataMember]
        public int LoanHistoryID { get; set; }

        [Length(250)] 
        [DataMember]
        public string FilePath { get; set; }

        [FK("LoanAgreementTemplate", "Id")]
        [DataMember]
        public int? LoanAgreementTemplateID { get; set; }

        [Length(LengthType.MAX)]
        [DataMember]
        public string AgreementModel { get; set; }
    }//class NL_LoanAgreements
}//ns
