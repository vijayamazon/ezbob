namespace Ezbob.Backend.ModelsWithDB.OpenPlatform {
	using System;
	using System.Runtime.Serialization;
	using Ezbob.Utils.dbutils;

	[DataContract(IsReference = true)]
	public class I_InvestorSystemBalance {
		[PK(true)]
		[DataMember]
		public int InvestorSystemBalanceID { get; set; }

		[FK("I_InvestorBankAccount", "InvestorBankAccountID")]
		[DataMember]
		public int InvestorBankAccountID { get; set; }

		[DataMember]
		public decimal? PreviousBalance { get; set; }

		[DataMember]
		public decimal? NewBalance { get; set; }

		[DataMember]
		public decimal? TransactionAmount { get; set; }

		[DataMember]
		public decimal? ServicingFeeAmount { get; set; }

		[DataMember]
		public DateTime Timestamp { get; set; }

		[FK("CashRequests", "Id")]
		[DataMember]
		public long? CashRequestID { get; set; }

		[FK("Loan", "Id")]
		[DataMember]
		public int? LoanID { get; set; }

		[FK("LoanTransaction", "Id")]
		[DataMember]
		public int? LoanTransactionID { get; set; }

		[DataMember]
		[Length(500)]
		public string Comment { get; set; }

		[DataMember]
		public int? UserID { get; set; }

		[DataMember]
		public DateTime? TransactionDate { get; set; }

		[DataMember]
		public long? NLOfferID { get; set; }

		[DataMember]
		public long? NLLoanID { get; set; }

		[DataMember]
		public long? NLPaymentID { get; set; }

	}//class I_InvestorSystemBalance
}//ns
