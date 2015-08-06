﻿namespace Ezbob.Backend.ModelsWithDB.NewLoan {
    using System.Runtime.Serialization;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
    public class NL_LoanFeePayments {
		[PK(true)]
        [DataMember]
		public long LoanFeePaymentID { get; set; }

        [FK("NL_LoanFees", "LoanFeeID")]
        [DataMember]
		public long LoanFeeID { get; set; }

        [FK("NL_Payments", "PaymentID")]
        [DataMember]
		public long PaymentID { get; set; }

        [DataMember]
        public decimal Amount { get; set; }
    }//class NL_LoanFeePayments
}//ns
