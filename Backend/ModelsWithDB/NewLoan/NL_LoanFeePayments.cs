namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System;
	using System.Runtime.Serialization;
	using Ezbob.Utils;
	using Ezbob.Utils.Attributes;
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
		[NonTraversable]
		[ExcludeFromToString]
		public bool NewFeePayment { get; set; }

		/*public override string ToString() {
			try {
				return ToStringTable();
			} catch (InvalidCastException invalidCastException) {
				Console.WriteLine(invalidCastException);
			}
			return string.Empty;
		}*/

		
	} // class NL_LoanFeePayments
} // ns
