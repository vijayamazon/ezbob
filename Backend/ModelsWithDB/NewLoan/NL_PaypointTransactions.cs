namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System;
	using System.Runtime.Serialization;
	using Ezbob.Utils.dbutils;

	[DataContract(IsReference = true)]
	public class NL_PaypointTransactions : AStringable {
		[PK(true)]
		[DataMember]
		public long PaypointTransactionID { get; set; }

		[FK("NL_Payments", "PaymentID")]
		[DataMember]
		public long PaymentID { get; set; }

		[DataMember]
		public DateTime TransactionTime { get; set; }

		[DataMember]
		public decimal Amount { get; set; }

		[Length(LengthType.MAX)]
		[DataMember]
		public string Notes { get; set; }

		[FK("NL_PaypointTransactionStatuses", "PaypointTransactionStatusID")]
		[DataMember]
		public int PaypointTransactionStatusID { get; set; }

		[Length(100)]
		[DataMember]
		public string PaypointUniqueID { get; set; }

		[FK("PayPointCard", "Id")]
		[DataMember]
		public int PaypointCardID { get; set; }

		[Length(32)]
		[DataMember]
		public string IP { get; set; }

	} // class NL_PaypointTransactions
} // ns
