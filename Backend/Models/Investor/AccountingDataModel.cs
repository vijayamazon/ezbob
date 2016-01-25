﻿namespace Ezbob.Backend.Models.Investor {
	using System.Runtime.Serialization;
	using Ezbob.Utils;

	[DataContract(IsReference = true)]
	public class AccountingDataModel {
		[DataMember]
		public int InvestorID { get; set; }

		[DataMember]
		public string InvestorType { get; set; }

		[DataMember]
		public string InvestorName { get; set; }

		[DataMember]
		public decimal OutstandingFunding { get; set; }

		[DataMember]
		public decimal AccumulatedRepayments { get; set; }

		[DataMember]
		public decimal TotalNonActiveAccumulatedRepayments { get; set; }

		[DataMember]
		public decimal ServicingFeeDiscount { get; set; }

		[DataMember]
		public bool IsInvestorActive { get; set; }

		[NonTraversable]
		public bool IsRepaymentsBankAccountActive { get { return RepaymentsBankAccountID.HasValue;} }

		[DataMember]
		public int? FundingBankAccountID { get; set; }

		[DataMember]
		public int? RepaymentsBankAccountID { get; set; }
	}
}
