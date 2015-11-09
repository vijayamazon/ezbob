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
        public bool PartialAutoCharging { get; set; }

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

		[DataMember]
		public virtual DateTime? StopLateFeeFromDate { get; set; }

		[DataMember]
		public virtual DateTime? StopLateFeeToDate { get; set; }

		/*/// <summary>
		/// prints data only
		/// to print headers line call base static PrintHeadersLine 
		/// </summary>
		/// <returns></returns>
		public override string ToString() {
			try {
				return ToStringTable();
			} catch (InvalidCastException invalidCastException) {
				Console.WriteLine(invalidCastException);
			}
			return string.Empty;
		}*/
		
	} // class NL_LoanOptions
} // ns
