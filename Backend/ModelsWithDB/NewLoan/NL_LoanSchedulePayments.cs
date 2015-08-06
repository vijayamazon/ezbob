namespace Ezbob.Backend.ModelsWithDB.NewLoan {
    using System.Runtime.Serialization;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
    public class NL_LoanSchedulePayments {
		[PK(true)]
        [DataMember]
		public long LoanSchedulePaymentID { get; set; }

        [FK("NL_LoanSchedules", "LoanScheduleID")]
        [DataMember]
		public long LoanScheduleID { get; set; }

        [FK("NL_Payments", "PaymentID")]
        [DataMember]
		public long PaymentID { get; set; }

        [DataMember]
        public decimal PrincipalPaid { get; set; }

        [DataMember]
        public decimal InterestPaid { get; set; }
    }//class NL_LoanSchedulePayments
}//ns
