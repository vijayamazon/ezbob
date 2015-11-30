namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System.Runtime.Serialization;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;

	[DataContract(IsReference = true)]
	public class NL_LoanFeePayments : AStringable {
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

		[DataMember]
		public decimal? ResetAmount { get; set; }

		[DataMember]
		[NonTraversable]
		public bool NewEntry { get; set; }

		//[DataMember]
		//[NonTraversable]
		//public bool ResetEntry { get; set; }
		
	} // class NL_LoanFeePayments
} // ns
