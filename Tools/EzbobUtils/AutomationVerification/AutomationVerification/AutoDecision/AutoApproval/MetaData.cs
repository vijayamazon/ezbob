namespace AutomationCalculator.AutoDecision.AutoApproval {
	using System;
	using System.Collections.Generic;
	using EZBob.DatabaseLib.Model.Database;
	using Newtonsoft.Json;

	/// <summary>
	/// Customer data read from DB that is used for auto approval check.
	/// One of the outputs of LoadAutoApprovalData sp.
	/// </summary>
	public class MetaData {
		public MetaData() {
			ValidationErrors = new List<string>();
		} // constructor

		public string RowType { get; set; }

		public string FirstName { get; set; }

		public string LastName { get; set; }

		public bool IsBrokerCustomer { get; set; }
		public bool IsLimitedCompanyType { get; set; }

		public int NumOfTodayAutoApproval { get; set; }
		public int NumOfYesterdayAutoApproval { get; set; }

		public int NumOfHourlyAutoApprovals { get; set; }
		public int NumOfLastHourAutoApprovals { get; set; }

		public decimal TodayLoanSum { get; set; }
		public decimal YesterdayLoanSum { get; set; }

		public int FraudStatusValue {
			get { return (int)FraudStatus; }
			set {
				FraudStatus = Enum.IsDefined(typeof (FraudStatus), value)
					? (FraudStatus)value
					: FraudStatus.UnderInvestigation;
			} // set
		} // FraudStatusValue

		public string AmlResult { get; set; }
		public string CustomerStatusName { get; set; }
		public bool CustomerStatusEnabled { get; set; }
		public int CompanyScore { get; set; }
		public int ConsumerScore { get; set; }
		public DateTime? IncorporationDate { get; set; }
		public DateTime DateOfBirth { get; set; }

		public int NumOfDefaultAccounts { get; set; }
		public int NumOfRollovers { get; set; }

		public int TotalLoanCount { get; set; }
		public int OpenLoanCount { get; set; }
		public decimal TakenLoanAmount { get; set; }
		public decimal RepaidPrincipal { get; set; }
		public decimal SetupFees { get; set; }

		public DateTime? OfferValidUntil { get; set; }
		public DateTime? OfferStart { get; set; }
		public bool EmailSendingBanned { get; set; }

		public string ExperianCompanyName { get; set; }

		public string EnteredCompanyName { get; set; }

		public int PreviousManualApproveCount { get; set; }

		public DateTime? CompanyDissolutionDate { get; set; }

		[JsonIgnore]
		public FraudStatus FraudStatus { get; private set; }

		[JsonIgnore]
		public List<string> ValidationErrors { get; private set; }

		[JsonIgnore]
		public int OfferLength {
			get {
				if (!OfferStart.HasValue || !OfferValidUntil.HasValue)
					return 0;

				return (int)(OfferValidUntil.Value - OfferStart.Value).TotalDays;
			} // get
		} // OfferLength

		[JsonIgnore]
		public decimal OutstandingPrincipal {
			get {
				return TakenLoanAmount - RepaidPrincipal - SetupFees;
			} // get
		} // OutstandingPrincipal

		[JsonIgnore]
		public decimal RepaidRatio {
			get {
				if (Math.Abs(TakenLoanAmount) < 0.00000001m)
					return 1;

				return (RepaidPrincipal + SetupFees) / TakenLoanAmount;
			} // get
		} // RepaidRatio

		public void Validate() {
			if (string.IsNullOrWhiteSpace(RowType))
				throw new Exception("Meta data was not loaded.");

			if (!OfferStart.HasValue || !OfferValidUntil.HasValue)
				ValidationErrors.Add("last offer start time/length not filled");
		} // Validate

		public void RestoreValidationErrors(IEnumerable<string> errors) {
			ValidationErrors.Clear();

			if (errors != null)
				ValidationErrors.AddRange(errors);
		} // RestoreValidationErrors
	} // class MetaData
} // namespace
