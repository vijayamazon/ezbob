namespace Ezbob.Backend.ModelsWithDB.NewLoan {
    using System;
    using System.Runtime.Serialization;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
    public class NL_LoanOptions {
		[PK(true)]
        [DataMember]
        public int LoanOptionsID { get; set; }

        [FK("NL_Loans", "LoanID")]
        [DataMember]
        public int LoanID { get; set; }

        [DataMember]
		public bool AutoCharge { get; set; }

		[DataMember]
		public DateTime? StopAutoChargeDate { get; set; }

		[DataMember]
		public bool AutoLateFees { get; set; }

		[DataMember]
		public DateTime? StopAutoLateFeesDate { get; set; }

		[DataMember]
		public bool AutoInterest { get; set; }

		[DataMember]
		public DateTime? StopAutoInterestDate { get; set; }

        [DataMember]
        public bool ReductionFee { get; set; }

        [DataMember]
        public bool LatePaymentNotification { get; set; }

        [Length(50)]
        [DataMember]
        public string CaisAccountStatus { get; set; }

        [Length(20)]
        [DataMember]
        public string ManualCaisFlag { get; set; }

        [DataMember]
        public bool EmailSendingAllowed { get; set; }

        [DataMember]
        public bool SmsSendingAllowed { get; set; }

        [DataMember]
        public bool MailSendingAllowed { get; set; }

        [DataMember]
        public int? UserID { get; set; }

        [DataMember]
        public DateTime InsertDate { get; set; }

        [DataMember]
        public bool IsActive { get; set; }

        [Length(LengthType.MAX)]
        [DataMember]
        public string Notes { get; set; }
    }//class NL_LoanOptions
}//ns
