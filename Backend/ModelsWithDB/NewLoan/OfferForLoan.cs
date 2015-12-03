namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System.Runtime.Serialization;

	[DataContract]
	public class OfferForLoan : AStringable {
		[DataMember]
		public long LoanLegalID { get; set; }

		[DataMember]
		public decimal LoanLegalAmount { get; set; }

		[DataMember]
		public int LoanLegalRepaymentPeriod { get; set; }

		[DataMember]
		public long OfferID { get; set; }

		[DataMember]
		public int LoanTypeID { get; set; }

		[DataMember]
		public int RepaymentIntervalTypeID { get; set; }

		[DataMember]
		public int LoanSourceID { get; set; }

		[DataMember]
		public int OfferRepaymentCount { get; set; }

		[DataMember]
		public decimal OfferAmount { get; set; }

		[DataMember]
		public decimal MonthlyInterestRate { get; set; }

		[DataMember]
		public bool SetupFeeAddedToLoan { get; set; }

		[DataMember]
		public decimal BrokerSetupFeePercent { get; set; }

		[DataMember]
		public int InterestOnlyRepaymentCount { get; set; }

		[DataMember]
		public int DiscountPlanID { get; set; }

		[DataMember]
		public int LoansCount { get; set; }

		[DataMember]
		public decimal AvailableAmount { get; set; }

		[DataMember]
		public string ExistsRefnums { get; set; }

        [DataMember]
        public string Error { get; set; }

	} // class OfferForLoan
} // namespace
