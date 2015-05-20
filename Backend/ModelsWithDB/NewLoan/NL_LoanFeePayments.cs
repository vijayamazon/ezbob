namespace Ezbob.Backend.ModelsWithDB.NewLoan {
    using System.Runtime.Serialization;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
    public class NL_LoanFeePayments {
		[PK(true)]
        [DataMember]
        public int LoanFeePaymentID { get; set; }

        [FK("NL_LoanFees", "LoanFeeID")]
        [DataMember]
        public int LoanFeeID { get; set; }

        [FK("NL_Payments", "PaymentID")]
        [DataMember]
        public int PaymentID { get; set; }

        [DataMember]
        public decimal Amount { get; set; }
    }//class NL_LoanFeePayments
}//ns
