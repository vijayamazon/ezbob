IF OBJECT_ID('GetNonLimitedDataForCompanyScore') IS NULL
	EXECUTE('CREATE PROCEDURE GetNonLimitedDataForCompanyScore AS SELECT 1')
GO

ALTER PROCEDURE GetNonLimitedDataForCompanyScore
	(@RefNumber NVARCHAR(50))
AS
BEGIN
	SET NOCOUNT ON;
	
	SELECT
		Id,
		BusinessName,
		Address1,
		Address2,
		Address3,
		Address4,
		Address5,
		Postcode,
		TelephoneNumber,
		PrincipalActivities,
		EarliestKnownDate,
		DateOwnershipCommenced,
		IncorporationDate,
		DateOwnershipCeased,
		LastUpdateDate,
		BankruptcyCountDuringOwnership,
		AgeOfMostRecentBankruptcyDuringOwnershipMonths,
		AssociatedBankruptcyCountDuringOwnership,
		AgeOfMostRecentAssociatedBankruptcyDuringOwnershipMonths,
		AgeOfMostRecentJudgmentDuringOwnershipMonths,
		TotalJudgmentCountLast12Months,
		TotalJudgmentValueLast12Months,
		TotalJudgmentCountLast13To24Months,
		TotalJudgmentValueLast13To24Months,
		ValueOfMostRecentAssociatedJudgmentDuringOwnership,
		TotalAssociatedJudgmentCountLast12Months,
		TotalAssociatedJudgmentValueLast12Months,
		TotalAssociatedJudgmentCountLast13To24Months,
		TotalAssociatedJudgmentValueLast13To24Months,
		TotalJudgmentCountLast24Months,
		TotalAssociatedJudgmentCountLast24Months,
		TotalJudgmentValueLast24Months,
		TotalAssociatedJudgmentValueLast24Months,
		SupplierName,
		FraudCategory,
		FraudCategoryDesc,
		NumberOfAccountsPlacedForCollection,
		ValueOfAccountsPlacedForCollection,
		NumberOfAccountsPlacedForCollectionLast2Years,
		AverageDaysBeyondTermsFor0To100,
		AverageDaysBeyondTermsFor101To1000,
		AverageDaysBeyondTermsFor1001To10000,
		AverageDaysBeyondTermsForOver10000,
		AverageDaysBeyondTermsForLast3MonthsOfDataReturned,
		AverageDaysBeyondTermsForLast6MonthsOfDataReturned,
		AverageDaysBeyondTermsForLast12MonthsOfDataReturned,
		CurrentAverageDebt,
		AverageDebtLast3Months,
		AverageDebtLast12Months,
		TelephoneNumberDN36,
		RiskScore,
		SearchType,
		SearchTypeDesc,
		CommercialDelphiScore,
		CreditRating,
		CreditLimit,
		ProbabilityOfDefaultScore,
		StabilityOdds,
		RiskBand,
		NumberOfProprietorsSearched,
		NumberOfProprietorsFound,
		Errors
	FROM 
		ExperianNonLimitedResults 
	WHERE 
		RefNumber = @RefNumber AND 
		IsActive = 1
END
GO
