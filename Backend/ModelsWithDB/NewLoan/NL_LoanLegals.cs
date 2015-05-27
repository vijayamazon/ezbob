namespace Ezbob.Backend.ModelsWithDB.NewLoan {
    using System;
    using System.Runtime.Serialization;
    using System.Text;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
    public class NL_LoanLegals {
		[PK(true)]
        [DataMember]
        public int LoanLegalID { get; set; }

        [FK("NL_Offers", "OfferID")]
        [DataMember]
        public int? OfferID { get; set; }
        
        [DataMember]
        public DateTime SignatureTime { get; set; }

         [DataMember]
        public int RepaymentPeriod { get; set; }

        [DataMember]
        public decimal Amount { get; set; }

        [DataMember]
        public bool? CreditActAgreementAgreed { get; set; }
        
        [DataMember]
        public bool? PreContractAgreementAgreed { get; set; }

        [DataMember]
        public bool? PrivateCompanyLoanAgreementAgreed { get; set; }

        [DataMember]
        public bool? GuarantyAgreementAgreed { get; set; }

        [DataMember]
        public bool? EUAgreementAgreed { get; set; }

        [DataMember]
        public bool? COSMEAgreementAgreed { get; set; }

        [DataMember]
        public bool? NotInBankruptcy { get; set; }

        [Length(128)]
        [DataMember]
        public string SignedName { get; set; }

	    /// <summary>
	    /// Returns a string that represents the current object.
	    /// </summary>
	    /// <returns>
	    /// A string that represents the current object.
	    /// </returns>
	    public override string ToString() {
			StringBuilder sb = new StringBuilder(this.GetType() + ": ");
			Type t = typeof(NL_LoanLegals);
			foreach (var prop in t.GetProperties()) {
				if (prop.GetValue(this) != null)
					sb.Append(prop.Name).Append(":").Append(prop.GetValue(this)).Append(@"; \n");
			}
			return sb.ToString();
	    }
    }//class NL_LoanLegals
}//ns
