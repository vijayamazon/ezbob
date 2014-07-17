namespace Ezbob.Backend.Models {
	using System;
	using System.Collections.Generic;
	using System.Runtime.Serialization;

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
	}
}
