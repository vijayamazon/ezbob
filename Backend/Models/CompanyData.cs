namespace Ezbob.Backend.Models {
	using System;
	using System.Collections.Generic;
	using System.Runtime.Serialization;

	[DataContract]
	public class PaymentPerformanceDetail
	{
		[DataMember]
		public string Code { get; set; }

		[DataMember]
		public int DaysBeyondTerms { get; set; }
	}

	[DataContract]
	public class PreviousSearch
	{
		[DataMember]
		public DateTime PreviousSearchDate { get; set; }

		[DataMember]
		public string EnquiryType { get; set; }

		[DataMember]
		public string EnquiryTypeDesc { get; set; }

		[DataMember]
		public string CreditRequired { get; set; }
	}

	[DataContract]
	public class TradingName
	{
		[DataMember]
		public string Name { get; set; }

		[DataMember]
		public string TradingIndicator { get; set; }

		[DataMember]
		public string TradingIndicatorDesc { get; set; }
	}

	[DataContract]
	public class CcjDetail
	{
		[DataMember]
		public string RecordType { get; set; }

		[DataMember]
		public string RecordTypeFullName { get; set; }

		[DataMember]
		public DateTime JudgementDate { get; set; }

		[DataMember]
		public string SatisfactionFlag { get; set; }

		[DataMember]
		public string SatisfactionFlagDesc { get; set; }

		[DataMember]
		public DateTime SatisfactionDate { get; set; }

		[DataMember]
		public string JudgmentType { get; set; }

		[DataMember]
		public string JudgmentTypeDesc { get; set; }
		
		[DataMember]
		public int JudgmentAmount { get; set; }

		[DataMember]
		public string Court { get; set; }

		[DataMember]
		public string CaseNumber { get; set; }

		[DataMember]
		public string NumberOfJudgmentNames { get; set; }

		[DataMember]
		public string NumberOfTradingNames { get; set; }

		[DataMember]
		public string LengthOfJudgmentName { get; set; }

		[DataMember]
		public string LengthOfTradingName { get; set; }

		[DataMember]
		public string LengthOfJudgmentAddress { get; set; }
		
		[DataMember]
		public string JudgementAddr1 { get; set; }

		[DataMember]
		public string JudgementAddr2 { get; set; }

		[DataMember]
		public string JudgementAddr3 { get; set; }

		[DataMember]
		public string JudgementAddr4 { get; set; }

		[DataMember]
		public string JudgementAddr5 { get; set; }

		[DataMember]
		public string PostCode { get; set; }

		[DataMember]
		public List<string> RegisteredAgainst { get; set; }

		[DataMember]
		public List<TradingName> TradingNames { get; set; }
	}

	[DataContract]
	public class BankruptcyDetail
	{
		[DataMember]
		public string BankruptcyName { get; set; }

		[DataMember]
		public string BankruptcyAddr1 { get; set; }

		[DataMember]
		public string BankruptcyAddr2 { get; set; }

		[DataMember]
		public string BankruptcyAddr3 { get; set; }

		[DataMember]
		public string BankruptcyAddr4 { get; set; }

		[DataMember]
		public string BankruptcyAddr5 { get; set; }

		[DataMember]
		public string PostCode { get; set; }

		[DataMember]
		public DateTime GazetteDate { get; set; }

		[DataMember]
		public string BankruptcyType { get; set; }

		[DataMember]
		public string BankruptcyTypeDesc { get; set; }
	}

	[DataContract]
	public class CompanyData
	{
		[DataMember]
		public bool IsLimited { get; set; }

		[DataMember]
		public string BusinessName { get; set; }

		[DataMember]
		public string Address1 { get; set; }

		[DataMember]
		public string Address2 { get; set; }

		[DataMember]
		public string Address3 { get; set; }

		[DataMember]
		public string Address4 { get; set; }

		[DataMember]
		public string Address5 { get; set; }

		[DataMember]
		public string Postcode { get; set; }

		[DataMember]
		public string TelephoneNumber { get; set; }

		[DataMember]
		public string PrincipalActivities { get; set; }

		[DataMember]
		public DateTime? EarliestKnownDate { get; set; }

		[DataMember]
		public DateTime? DateOwnershipCommenced { get; set; }

		[DataMember]
		public DateTime? IncorporationDate { get; set; }

		[DataMember]
		public DateTime? DateOwnershipCeased { get; set; }

		[DataMember]
		public DateTime? LastUpdateDate { get; set; }

		[DataMember]
		public int? BankruptcyCountDuringOwnership { get; set; }

		[DataMember]
		public int? AgeOfMostRecentBankruptcyDuringOwnershipMonths { get; set; }

		[DataMember]
		public int? AssociatedBankruptcyCountDuringOwnership { get; set; }

		[DataMember]
		public int? AgeOfMostRecentAssociatedBankruptcyDuringOwnershipMonths { get; set; }

		[DataMember]
		public int? AgeOfMostRecentJudgmentDuringOwnershipMonths { get; set; }

		[DataMember]
		public int? TotalJudgmentCountLast12Months { get; set; }

		[DataMember]
		public int? TotalJudgmentValueLast12Months { get; set; }

		[DataMember]
		public int? TotalJudgmentCountLast13To24Months { get; set; }

		[DataMember]
		public int? TotalJudgmentValueLast13To24Months { get; set; }

		[DataMember]
		public int? ValueOfMostRecentAssociatedJudgmentDuringOwnership { get; set; }

		[DataMember]
		public int? TotalAssociatedJudgmentCountLast12Months { get; set; }

		[DataMember]
		public int? TotalAssociatedJudgmentValueLast12Months { get; set; }

		[DataMember]
		public int? TotalAssociatedJudgmentCountLast13To24Months { get; set; }

		[DataMember]
		public int? TotalAssociatedJudgmentValueLast13To24Months { get; set; }

		[DataMember]
		public int? TotalJudgmentCountLast24Months { get; set; }

		[DataMember]
		public int? TotalAssociatedJudgmentCountLast24Months { get; set; }

		[DataMember]
		public int? TotalJudgmentValueLast24Months { get; set; }

		[DataMember]
		public int? TotalAssociatedJudgmentValueLast24Months { get; set; }

		[DataMember]
		public string SupplierName { get; set; }

		[DataMember]
		public string FraudCategory { get; set; }

		[DataMember]
		public string FraudCategoryDesc { get; set; }

		[DataMember]
		public int? NumberOfAccountsPlacedForCollection { get; set; }

		[DataMember]
		public int? ValueOfAccountsPlacedForCollection { get; set; }

		[DataMember]
		public int? NumberOfAccountsPlacedForCollectionLast2Years { get; set; }

		[DataMember]
		public int? AverageDaysBeyondTermsFor0To100 { get; set; }

		[DataMember]
		public int? AverageDaysBeyondTermsFor101To1000 { get; set; }

		[DataMember]
		public int? AverageDaysBeyondTermsFor1001To10000 { get; set; }

		[DataMember]
		public int? AverageDaysBeyondTermsForOver10000 { get; set; }

		[DataMember]
		public int? AverageDaysBeyondTermsForLast3MonthsOfDataReturned { get; set; }

		[DataMember]
		public int? AverageDaysBeyondTermsForLast6MonthsOfDataReturned { get; set; }

		[DataMember]
		public int? AverageDaysBeyondTermsForLast12MonthsOfDataReturned { get; set; }

		[DataMember]
		public int? CurrentAverageDebt { get; set; }

		[DataMember]
		public int? AverageDebtLast3Months { get; set; }

		[DataMember]
		public int? AverageDebtLast12Months { get; set; }

		[DataMember]
		public string TelephoneNumberDN36 { get; set; }

		[DataMember]
		public int? RiskScore { get; set; }

		[DataMember]
		public string SearchType { get; set; }

		[DataMember]
		public string SearchTypeDesc { get; set; }

		[DataMember]
		public int? CommercialDelphiScore { get; set; }

		[DataMember]
		public string CreditRating { get; set; }

		[DataMember]
		public string CreditLimit { get; set; }

		[DataMember]
		public decimal? ProbabilityOfDefaultScore { get; set; }

		[DataMember]
		public string StabilityOdds { get; set; }

		[DataMember]
		public string RiskBand { get; set; }

		[DataMember]
		public int? NumberOfProprietorsSearched { get; set; }

		[DataMember]
		public int? NumberOfProprietorsFound { get; set; }

		[DataMember]
		public string Errors { get; set; }

		[DataMember]
		public List<string> SicCodes { get; set; }

		[DataMember]
		public List<string> SicDescs { get; set; }

		[DataMember]
		public List<BankruptcyDetail> BankruptcyDetails { get; set; }

		[DataMember]
		public List<CcjDetail> CcjDetails { get; set; }

		[DataMember]
		public List<PreviousSearch> PreviousSearches { get; set; }

		[DataMember]
		public List<PaymentPerformanceDetail> PaymentPerformanceDetails { get; set; }
	}
}
