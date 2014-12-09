namespace EzServiceConfiguration {

	public enum QuickOfferEnabledStatus {
		Disabled = 0,
		Enabled = 1,
		Silent = 2,
	} // enum QuickOfferEnabledStatus

	public abstract class QuickOfferConfigurationData : AConfigurationData {

		public virtual QuickOfferEnabledStatus Enabled { get; protected set; }
		public virtual int CompanySeniorityMonths { get; protected set; }
		public virtual int ApplicantMinAgeYears { get; protected set; }
		public virtual int NoDefaultsInLastMonths { get; protected set; }
		public virtual int AmlMin { get; protected set; }
		public virtual int PersonalScoreMin { get; protected set; }
		public virtual int BusinessScoreMin { get; protected set; }
		public virtual int MaxLoanCountPerDay { get; protected set; }
		public virtual int MaxIssuedValuePerDay { get; protected set; }
		public virtual int OfferDurationHours { get; protected set; }
		public virtual int MinOfferAmount { get; protected set; }
		public virtual decimal OfferCapPct { get; protected set; }
		public virtual int ImmediateMaxAmount { get; protected set; }
		public virtual int ImmediateTermMonths { get; protected set; }
		public virtual decimal ImmediateInterestRate { get; protected set; }
		public virtual decimal ImmediateSetupFee { get; protected set; }
		public virtual int PotentialMaxAmount { get; protected set; }
		public virtual int PotentialTermMonths { get; protected set; }
		public virtual decimal PotentialSetupFee { get; protected set; }
		public virtual string OfferAmountCalculator { get; protected set; }
		public virtual string PriceCalculator { get; protected set; }

		public abstract decimal OfferAmountPct(int nBusinessScore);
		public abstract decimal LoanPct(int nBusinessScore, decimal nRequestedAmount);

		protected override string InvalidExceptionMessage {
			get { return "Invalid quick offer configuration has been loaded from DB."; } // get
		} // InvalidExceptionMessage

		protected override bool IsValid() {
			return
				(CompanySeniorityMonths >= 0) &&
				(ApplicantMinAgeYears >= 0) &&
				(NoDefaultsInLastMonths >= 0) &&
				(AmlMin >= 0) &&
				(PersonalScoreMin >= 0) &&
				(BusinessScoreMin >= 0) &&
				(MaxLoanCountPerDay >= 0) &&
				(MaxIssuedValuePerDay >= 0) &&
				(OfferDurationHours >= 0) &&
				(MinOfferAmount >= 0) &&
				(OfferCapPct >= 0) &&
				(ImmediateMaxAmount >= 0) &&
				(ImmediateTermMonths >= 0) &&
				(ImmediateInterestRate >= 0) &&
				(ImmediateSetupFee >= 0) &&
				(PotentialMaxAmount >= 0) &&
				(PotentialTermMonths >= 0) &&
				(PotentialSetupFee >= 0) &&
				!string.IsNullOrWhiteSpace(OfferAmountCalculator) &&
				!string.IsNullOrWhiteSpace(PriceCalculator);
		} // IsValid

	} // class QuickOfferConfigurationData

} // namespace EzServiceConfiguration
