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
	    public bool PartialAutoCharging { get; set; } = true;
		[DataMember]
		public bool LatePaymentNotification { get; set; } = true;
		[Length(50)]
		[DataMember]
		public string CaisAccountStatus { get; set; }
		[Length(20)]
		[DataMember]
		public string ManualCaisFlag { get; set; }
		[DataMember]
		public bool EmailSendingAllowed { get; set; } = true;
		[DataMember]
		public bool SmsSendingAllowed { get; set; } = true;

		[DataMember]
		public bool MailSendingAllowed { get; set; } = true;
		[DataMember]
		public int? UserID { get; set; }
		[DataMember]
		public DateTime InsertDate { get; set; }
		public bool IsActive { get; set; }
		[Length(LengthType.MAX)]
		[DataMember]
		public string Notes { get; set; }
		[DataMember]
		public virtual DateTime? StopLateFeeFromDate { get; set; }
		[DataMember]
		public virtual DateTime? StopLateFeeToDate { get; set; }
	} // class NL_LoanOptions
} // ns
