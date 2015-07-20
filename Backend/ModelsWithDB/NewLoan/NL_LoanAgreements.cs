namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System;
	using System.Runtime.Serialization;
    using System.Text;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
    public class NL_LoanAgreements {
		[PK(true)]
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

	   
	    public override string ToString() {
			StringBuilder sb = new StringBuilder(this.GetType().Name+": ");
			Type t = typeof(NL_LoanAgreements);
			foreach (var prop in t.GetProperties()) {
				if (prop.GetValue(this) != null)
					sb.Append(prop.Name).Append(": ").Append(prop.GetValue(this)).Append("; \n");
			}
			return sb.ToString();
	    }
    }//class NL_LoanAgreements
}//ns
