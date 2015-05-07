IF OBJECT_ID('InsertNonLimitedResult') IS NULL
	EXECUTE('CREATE PROCEDURE InsertNonLimitedResult AS SELECT 1')
GO

ALTER PROCEDURE InsertNonLimitedResult
	(@RefNumber NVARCHAR(50),
	 @ServiceLogId INT,
	 @Created DATETIME,		
	 @BusinessName NVARCHAR(75),
	 @Address1 NVARCHAR(30),
	 @Address2 NVARCHAR(30),
	 @Address3 NVARCHAR(30),
	 @Address4 NVARCHAR(30),
	 @Address5 NVARCHAR(30),
	 @Postcode NVARCHAR(8),
	 @TelephoneNumber NVARCHAR(20),
	 @PrincipalActivities NVARCHAR(75),
	 @EarliestKnownDate DATETIME,
	 @DateOwnershipCommenced DATETIME,
	 @IncorporationDate DATETIME,
	 @DateOwnershipCeased DATETIME,
	 @LastUpdateDate DATETIME,		
	 @BankruptcyCountDuringOwnership INT,
	 @AgeOfMostRecentBankruptcyDuringOwnershipMonths INT,
	 @AssociatedBankruptcyCountDuringOwnership INT,
	 @AgeOfMostRecentAssociatedBankruptcyDuringOwnershipMonths INT,		
	 @AgeOfMostRecentJudgmentDuringOwnershipMonths INT,
	 @TotalJudgmentCountLast12Months INT,
	 @TotalJudgmentValueLast12Months INT,
	 @TotalJudgmentCountLast13To24Months INT,
	 @TotalJudgmentValueLast13To24Months INT,
	 @ValueOfMostRecentAssociatedJudgmentDuringOwnership INT,
	 @TotalAssociatedJudgmentCountLast12Months INT,
	 @TotalAssociatedJudgmentValueLast12Months INT,
	 @TotalAssociatedJudgmentCountLast13To24Months INT,
	 @TotalAssociatedJudgmentValueLast13To24Months INT,
	 @TotalJudgmentCountLast24Months INT,
	 @TotalAssociatedJudgmentCountLast24Months INT,
	 @TotalJudgmentValueLast24Months INT,
	 @TotalAssociatedJudgmentValueLast24Months INT,		
	 @SupplierName NVARCHAR(16),
	 @FraudCategory NVARCHAR(2),
	 @FraudCategoryDesc NVARCHAR(200),		
	 @NumberOfAccountsPlacedForCollection INT,
	 @ValueOfAccountsPlacedForCollection INT,
	 @NumberOfAccountsPlacedForCollectionLast2Years INT,
	 @AverageDaysBeyondTermsFor0To100 INT,
	 @AverageDaysBeyondTermsFor101To1000 INT,
	 @AverageDaysBeyondTermsFor1001To10000 INT,
	 @AverageDaysBeyondTermsForOver10000 INT,
	 @AverageDaysBeyondTermsForLast3MonthsOfDataReturned INT,
	 @AverageDaysBeyondTermsForLast6MonthsOfDataReturned INT,
	 @AverageDaysBeyondTermsForLast12MonthsOfDataReturned INT,
	 @CurrentAverageDebt INT,
	 @AverageDebtLast3Months INT,
	 @AverageDebtLast12Months INT,
	 @TelephoneNumberDN36 NVARCHAR(20),		
	 @RiskScore INT,		
	 @SearchType NVARCHAR(1),
	 @SearchTypeDesc NVARCHAR(25),
	 @CommercialDelphiScore INT,
	 @CreditRating NVARCHAR(8),
	 @CreditLimit NVARCHAR(8),
	 @ProbabilityOfDefaultScore DECIMAL(18,6),
	 @StabilityOdds NVARCHAR(10),
	 @RiskBand NVARCHAR(1),
	 @NumberOfProprietorsSearched INT,
	 @NumberOfProprietorsFound INT,		
	 @Errors NVARCHAR(MAX),
	 @RiskText NVARCHAR(70),
	 @CreditText NVARCHAR(560),
	 @ConcludingText NVARCHAR(200),
	 @NocText NVARCHAR(200),
	 @PossiblyRelatedDataText NVARCHAR(200))
AS
BEGIN
	SET NOCOUNT ON;
	
	UPDATE ExperianNonLimitedResults SET IsActive = 0 WHERE RefNumber = @RefNumber
	
	INSERT INTO ExperianNonLimitedResults
		(RefNumber,
		 ServiceLogId,
		 Created,		
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
		 Errors,
		 IsActive,
		 RiskText,
		 CreditText,
		 ConcludingText,
		 NocText,
		 PossiblyRelatedDataText)
	VALUES
		(@RefNumber,
		 @ServiceLogId,
		 @Created,		
		 @BusinessName,
		 @Address1,
		 @Address2,
		 @Address3,
		 @Address4,
		 @Address5,
		 @Postcode,
		 @TelephoneNumber,
		 @PrincipalActivities,
		 @EarliestKnownDate,
		 @DateOwnershipCommenced,
		 @IncorporationDate,
		 @DateOwnershipCeased,
		 @LastUpdateDate,		
		 @BankruptcyCountDuringOwnership,
		 @AgeOfMostRecentBankruptcyDuringOwnershipMonths,
		 @AssociatedBankruptcyCountDuringOwnership,
		 @AgeOfMostRecentAssociatedBankruptcyDuringOwnershipMonths,		
		 @AgeOfMostRecentJudgmentDuringOwnershipMonths,
		 @TotalJudgmentCountLast12Months,
		 @TotalJudgmentValueLast12Months,
		 @TotalJudgmentCountLast13To24Months,
		 @TotalJudgmentValueLast13To24Months,
		 @ValueOfMostRecentAssociatedJudgmentDuringOwnership,
		 @TotalAssociatedJudgmentCountLast12Months,
		 @TotalAssociatedJudgmentValueLast12Months,
		 @TotalAssociatedJudgmentCountLast13To24Months,
		 @TotalAssociatedJudgmentValueLast13To24Months,
		 @TotalJudgmentCountLast24Months,
		 @TotalAssociatedJudgmentCountLast24Months,
		 @TotalJudgmentValueLast24Months,
		 @TotalAssociatedJudgmentValueLast24Months,		
		 @SupplierName,
		 @FraudCategory,
		 @FraudCategoryDesc,		
		 @NumberOfAccountsPlacedForCollection,
		 @ValueOfAccountsPlacedForCollection,
		 @NumberOfAccountsPlacedForCollectionLast2Years,
		 @AverageDaysBeyondTermsFor0To100,
		 @AverageDaysBeyondTermsFor101To1000,
		 @AverageDaysBeyondTermsFor1001To10000,
		 @AverageDaysBeyondTermsForOver10000,
		 @AverageDaysBeyondTermsForLast3MonthsOfDataReturned,
		 @AverageDaysBeyondTermsForLast6MonthsOfDataReturned,
		 @AverageDaysBeyondTermsForLast12MonthsOfDataReturned,
		 @CurrentAverageDebt,
		 @AverageDebtLast3Months,
		 @AverageDebtLast12Months,
		 @TelephoneNumberDN36,		
		 @RiskScore,		
		 @SearchType,
		 @SearchTypeDesc,
		 @CommercialDelphiScore,
		 @CreditRating,
		 @CreditLimit,
		 @ProbabilityOfDefaultScore,
		 @StabilityOdds,
		 @RiskBand,
		 @NumberOfProprietorsSearched,
		 @NumberOfProprietorsFound,		
		 @Errors,
		 1,
		 @RiskText,
		 @CreditText,
		 @ConcludingText,
		 @NocText,
		 @PossiblyRelatedDataText)
		
	SELECT CAST(@@IDENTITY AS INT) AS NewId
END
GO
