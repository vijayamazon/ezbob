namespace Ezbob.Backend.Models {
	using System;
	using System.Runtime.Serialization;
	
	[DataContract(IsReference = true)]
	public class LoanScheduleItemModel {
		[DataMember]
		public long Id { get; set; }

		[DataMember]
		public DateTime Date { get; set; }

		[DataMember]
		public DateTime PrevInstallmentDate { get; set; }

		[DataMember]
		public decimal RepaymentAmount { get; set; }

		[DataMember]
		public decimal Interest { get; set; }

		[DataMember]
		public decimal InterestPaid { get; set; }

		[DataMember]
		public string Status { get; set; }

		[DataMember]
		public decimal LateCharges { get; set; }

		[DataMember]
		public decimal AmountDue { get; set; }

		[DataMember]
		public decimal LoanRepayment { get; set; }

		[DataMember]
		public string StatusDescription { get; set; }

		[DataMember]
		public decimal Balance { get; set; }

		[DataMember]
		public decimal BalanceBeforeRepayment { get; set; }

		[DataMember]
		public decimal Fees { get; set; }

		[DataMember]
		public decimal InterestRate { get; set; }
	}
}