namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System;
	using System.Runtime.Serialization;
	using Ezbob.Utils.dbutils;

	[DataContract(IsReference = true)]
	public class NL_FundTransfers : AStringable {
		[PK(true)]
		[DataMember]
		public long FundTransferID { get; set; }

		[FK("NL_Loans", "LoanID")]
		[DataMember]
		public long LoanID { get; set; }

		[DataMember]
		public decimal Amount { get; set; }

		[DataMember]
		public DateTime TransferTime { get; set; }

		[FK("NL_FundTransferStatuses", "FundTransferStatusID")]
		[DataMember]
		public int FundTransferStatusID { get; set; }

		[FK("LoanTransactionMethod", "Id")]
		[DataMember]
		public int LoanTransactionMethodID { get; set; }

		[DataMember]
		public DateTime? DeletionTime { get; set; }

		[FK("Security_User", "UserId")]
		[DataMember]
		public int? DeletedByUserID { get; set; }
	} // class NL_FundTransfers
} // ns