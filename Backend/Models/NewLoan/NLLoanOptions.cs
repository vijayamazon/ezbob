namespace Ezbob.Backend.Models.NewLoan {
	using System;
	using System.Runtime.Serialization;

	[DataContract(IsReference = true)]
	public class NLLoanOptions : AStringable {
		[DataMember]
		public long? LoanOptionsID { get; set; }

		[DataMember]
		public long? LoanID { get; set; }

		[DataMember]
		public bool? AutoCharge { get; set; }

		[DataMember]
		public DateTime? StopAutoChargeDate { get; set; }

		[DataMember]
		public bool? AutoLateFees { get; set; }

		[DataMember]
		public DateTime? StopAutoLateFeesDate { get; set; }

		[DataMember]
		public bool? AutoInterest { get; set; }

		[DataMember]
		public DateTime? StopAutoInterestDate { get; set; }

		[DataMember]
		public bool? ReductionFee { get; set; }

		[DataMember]
		public bool? LatePaymentNotification { get; set; }

		[DataMember]
		public string CaisAccountStatus { get; set; }

		[DataMember]
		public string ManualCaisFlag { get; set; }

		[DataMember]
		public bool? EmailSendingAllowed { get; set; }

		[DataMember]
		public bool? SmsSendingAllowed { get; set; }

		[DataMember]
		public bool? MailSendingAllowed { get; set; }

		[DataMember]
		public int? UserID { get; set; }

		[DataMember]
		public DateTime? InsertDate { get; set; }

		[DataMember]
		public bool? IsActive { get; set; }

		[DataMember]
		public string Notes { get; set; }
	} // class NL_LoanOptions
} // ns
