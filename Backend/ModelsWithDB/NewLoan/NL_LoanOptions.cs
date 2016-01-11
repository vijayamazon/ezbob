namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System;
	using System.Runtime.Serialization;
	using Ezbob.Utils.dbutils;

	[DataContract(IsReference = true)]
	public class NL_LoanOptions : AStringable {
	    [PK(true)]
		[DataMember]
		public long LoanOptionsID { get; set; }

		[FK("NL_Loans", "LoanID")]
		[DataMember]
		public long LoanID { get; set; }

		[DataMember]
		public DateTime? StopAutoChargeDate { get; set; }

	    [DataMember]
	    public bool PartialAutoCharging{
            get { return this.partialAutoCharging; }
            set { this.partialAutoCharging = value; }
	    }

	    [DataMember]
        public bool LatePaymentNotification{
            get { return this.latePaymentNotification; }
            set { this.latePaymentNotification = value; }
        }

		[Length(50)]
		[DataMember]
		public string CaisAccountStatus { get; set; }

		[Length(20)]
		[DataMember]
		public string ManualCaisFlag { get; set; }

		[DataMember]
        public bool EmailSendingAllowed{
            get { return this.emailSendingAllowed; }
            set { this.emailSendingAllowed = value; }
        }

		[DataMember]
        public bool SmsSendingAllowed{
            get { return this.smsSendingAllowed; }
            set { this.smsSendingAllowed = value; }
        }

		[DataMember]
        public bool MailSendingAllowed{
            get { return this.mailSendingAllowed; }
            set { this.mailSendingAllowed = value; }
        }

		[DataMember]
		public int? UserID { get; set; }

		[DataMember]
		public DateTime InsertDate { get; set; }

		[DataMember]
		public bool IsActive { get; set; }

		[Length(LengthType.MAX)]
		[DataMember]
		public string Notes { get; set; }

		[DataMember]
		public virtual DateTime? StopLateFeeFromDate { get; set; }

		[DataMember]
		public virtual DateTime? StopLateFeeToDate { get; set; }

		private bool partialAutoCharging = true;
		private bool latePaymentNotification = true;
		private bool emailSendingAllowed = true;
		private bool smsSendingAllowed = true;
		private bool mailSendingAllowed = true;
	} // class NL_LoanOptions
} // ns
