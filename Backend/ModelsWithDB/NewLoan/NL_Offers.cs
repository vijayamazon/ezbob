namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System;
	using System.Runtime.Serialization;
	using Ezbob.Utils.dbutils;

	[DataContract(IsReference = true)]
	public class NL_Offers : AStringable {
		[PK(true)]
		public long OfferID { get; set; }

		[FK("NL_Decisions", "DecisionID")]
		[DataMember]
		public long DecisionID { get; set; }

		[FK("LoanType", "Id")]
		[DataMember]
		public int LoanTypeID { get; set; }

		[FK("NL_RepaymentIntervalTypes", "RepaymentIntervalTypeID")]
		[DataMember]
		public int RepaymentIntervalTypeID { get; set; }

		[FK("LoanSource", "LoanSourceID")]
		[DataMember]
		public int LoanSourceID { get; set; }

		[DataMember]
		public DateTime StartTime { get; set; }

		[DataMember]
		public DateTime EndTime { get; set; }

		[DataMember]
		public int RepaymentCount { get; set; }

		[DataMember]
		public decimal Amount { get; set; }

		[DataMember]
		public decimal MonthlyInterestRate { get; set; }

		[DataMember]
		public DateTime CreatedTime { get; set; }

		[DataMember]
		public decimal? BrokerSetupFeePercent { get; set; }

		[DataMember]
		public bool SetupFeeAddedToLoan { get; set; }

		[Length(LengthType.MAX)]
		[DataMember]
		public string Notes { get; set; }

		[DataMember]
		public int InterestOnlyRepaymentCount { get; set; }

		[FK("NL_DiscountPlans", "DiscountPlanID")]
		[DataMember]
		public int? DiscountPlanID { get; set; }

		[DataMember]
		public bool IsLoanTypeSelectionAllowed { get; set; }

		[DataMember]
		public bool SendEmailNotification { get; set; }

		[DataMember]
		public bool IsRepaymentPeriodSelectionAllowed { get; set; }

		[DataMember]
		public bool IsAmountSelectionAllowed { get; set; }

	} // class NL_Offers
} // ns
