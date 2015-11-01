namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System;
	using System.Runtime.Serialization;
	using Ezbob.Utils;
	using Ezbob.Utils.Attributes;
	using Ezbob.Utils.dbutils;

	[DataContract(IsReference = true)]
	public class NL_LoanSchedulePayments : AStringable {
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

		[DataMember]
		[NonTraversable]
		[ExcludeFromToString]
		public bool NewSchedulePayment { get; set; }

		[DataMember]
		[NonTraversable]
		[ExcludeFromToString]
		public DateTime? PaymentDate { get; set; }

		/*public override string ToString() {
			try {
				return ToStringTable();
			} catch (InvalidCastException invalidCastException) {
				//Console.WriteLine(invalidCastException);
			}
			return string.Empty;
		}*/
	} // class NL_LoanSchedulePayments
} // ns
