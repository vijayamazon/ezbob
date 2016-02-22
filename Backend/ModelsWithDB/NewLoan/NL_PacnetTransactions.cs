namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System;
	using System.Runtime.Serialization;
	using DbConstants;
	using Ezbob.Utils.Attributes;
	using Ezbob.Utils.dbutils;

	[DataContract(IsReference = true)]
	public class NL_PacnetTransactions : AStringable {
		[PK(true)]
		[DataMember]
		public long PacnetTransactionID { get; set; }

		[FK("NL_FundTransfers", "FundTransferID")]
		[DataMember]
		public long FundTransferID { get; set; }

		[DataMember]
		public DateTime TransactionTime { get; set; }

		[DataMember]
		public decimal Amount { get; set; }

		[Length(LengthType.MAX)]
		[DataMember]
		public string Notes { get; set; }

		[FK("NL_PacnetTransactionStatuses", "PacnetTransactionStatusID")]
		[DataMember]
		[EnumName(typeof(NLPacnetTransactionStatuses))]
		public int PacnetTransactionStatusID { get; set; }

		[DataMember]
		public DateTime StatusUpdatedTime { get; set; }

		[Length(100)]
		[DataMember]
		public string TrackingNumber { get; set; }
	} // class NL_PacnetTransactions
} // ns
