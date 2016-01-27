namespace Ezbob.Backend.Models.Investor {
	using System;
	using System.Runtime.Serialization;

	[DataContract(IsReference = true)]
	public class TransactionsDataModel {
		
		[DataMember]
		public int TransactionID { get; set; }

		[DataMember]
		public DateTime TransactionDate { get; set; }

		[DataMember]
		public decimal TransactionAmount { get; set; }

		[DataMember]
		public decimal? PreviousAmount { get; set; }

		[DataMember]
		public decimal? NewAmount { get; set; }

		[DataMember]
		public string BankTransactionRef { get; set; }

		[DataMember]
		public decimal BankAccountNumber { get; set; }

		[DataMember]
		public string BankAccountName { get; set; }

		[DataMember]
		public bool IsBankAccountActive { get; set; }

		[DataMember]
		public string Comment { get; set; }

		[DataMember]
		public DateTime Timestamp { get; set; }
	}
}


