﻿namespace Ezbob.Backend.ModelsWithDB.NewLoan {
    using System;
    using System.Runtime.Serialization;
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
    }//class NL_LoanLegals
}//ns
