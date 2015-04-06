namespace Reports.LoanStats {
	using System;
	using System.Collections.Generic;

	public class LoanStatsReportEntry {
		public LoanStatsReportEntry() {
			MarketplaceCount = new SortedDictionary<int, int>();
		} // constructor

		public string IsFirstLoan { get; set; }
		public int ClientLoanOrderNo { get; set; }
		public string TypeOfLoan { get; set; }
		public int CustomerSelection { get; set; }
		public string DiscountPlan { get; set; }
		public string Offline { get; set; }
		public int? LoanID { get; set; }
		public int ClientID { get; set; }
		public string ClientName { get; set; }
		public DateTime DateFirstApproved { get; set; }
		public DateTime DateLastApproved { get; set; }
		public string NewOrOldClient { get; set; }
		public Decimal LoanOffered { get; set; }
		public Decimal InterestRate { get; set; }
		public Decimal? LoanIssued { get; set; }
		public string IsLoanIssued { get; set; }
		public DateTime? LoanIssueDate { get; set; }
		public int? LoanDuration { get; set; }
		public int CreditScore { get; set; }
		public int TotalAnnualTurnover { get; set; }
		public string Medal { get; set; }
		public SortedDictionary<int, int> MarketplaceCount { get; private set; }
		public Decimal PaypalTotal { get; set; }
		public string Gender { get; set; }
		public int YearOfBirth { get; set; }
		public int FamilyStatus { get; set; }
		public int? HomeOwnership { get; set; }
		public int TypeOfBusiness { get; set; }
		public string SourceRef { get; set; }
		public string Category1 { get; set; }
		public string Category2 { get; set; }
		public string Category3 { get; set; }
		public string Region { get; set; }
	} // class LoanStatsReportEntry
} // namespace
