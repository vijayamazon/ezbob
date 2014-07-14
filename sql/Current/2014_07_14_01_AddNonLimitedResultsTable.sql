IF OBJECT_ID('ExperianNonLimitedResults') IS NOT NULL
BEGIN
	IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'TelephoneNumber' and Object_ID = Object_ID(N'ExperianNonLimitedResults'))
	BEGIN
		DROP TABLE ExperianNonLimitedResultsScoreHistory
		DROP TABLE ExperianNonLimitedResults
	END
END
GO

IF OBJECT_ID('ExperianNonLimitedResultScoreHistory') IS NULL
BEGIN
	CREATE TABLE ExperianNonLimitedResultScoreHistory (
		Id INT IDENTITY NOT NULL,
		ExperianNonLimitedResultId INT,
		RiskScore INT,
		Date DATETIME
	)
END
GO

IF OBJECT_ID('ExperianNonLimitedResultSicCodes') IS NULL
BEGIN
	CREATE TABLE ExperianNonLimitedResultSicCodes (
		Id INT IDENTITY NOT NULL,
		ExperianNonLimitedResultId INT,
		Code NVARCHAR(5),
		Description NVARCHAR(200)
	)
END
GO

IF OBJECT_ID('ExperianNonLimitedResultBankruptcyDetails') IS NULL
BEGIN
	CREATE TABLE ExperianNonLimitedResultBankruptcyDetails (
		Id INT IDENTITY NOT NULL,
		ExperianNonLimitedResultId INT,
		BankruptcyName NVARCHAR(75),
		BankruptcyAddr1 NVARCHAR(30),
		BankruptcyAddr2 NVARCHAR(30),
		BankruptcyAddr3 NVARCHAR(30),
		BankruptcyAddr4 NVARCHAR(30),
		BankruptcyAddr5 NVARCHAR(30),
		PostCode NVARCHAR(8),
		GazetteDate DATETIME,
		BankruptcyType NVARCHAR(3),
		BankruptcyTypeDesc NVARCHAR(20)
	)
END
GO

IF OBJECT_ID('ExperianNonLimitedResultCcjDetails') IS NULL
BEGIN
	CREATE TABLE ExperianNonLimitedResultCcjDetails (
		Id INT IDENTITY NOT NULL,
		ExperianNonLimitedResultId INT,		
		RecordType NVARCHAR(1),
		RecordTypeFullName NVARCHAR(10),
		JudgementDate DATETIME,
		SatisfactionFlag NVARCHAR(1),
		SatisfactionFlagDesc NVARCHAR(15),
		SatisfactionDate DATETIME,
		JudgmentType NVARCHAR(3),
		JudgmentTypeDesc NVARCHAR(35),
		JudgmentAmount INT,
		Court NVARCHAR(30),
		CaseNumber NVARCHAR(11),
		NumberOfJudgmentNames NVARCHAR(2),
		NumberOfTradingNames NVARCHAR(2),
		LengthOfJudgmentName NVARCHAR(3),
		LengthOfTradingName NVARCHAR(3),
		LengthOfJudgmentAddress NVARCHAR(3),
		JudgementAddr1 NVARCHAR(30),
		JudgementAddr2 NVARCHAR(30),
		JudgementAddr3 NVARCHAR(30),
		JudgementAddr4 NVARCHAR(30),
		JudgementAddr5 NVARCHAR(30),
		PostCode NVARCHAR(8)
	)
END
GO

IF OBJECT_ID('ExperianNonLimitedResultCcjRegisteredAgainst') IS NULL
BEGIN
	CREATE TABLE ExperianNonLimitedResultCcjRegisteredAgainst (
		Id INT IDENTITY NOT NULL,
		ExperianNonLimitedResultCcjDetailsId INT,		
		Name NVARCHAR(75)
	)
END
GO

IF OBJECT_ID('ExperianNonLimitedResultCcjTradingNames') IS NULL
BEGIN
	CREATE TABLE ExperianNonLimitedResultCcjTradingNames (
		Id INT IDENTITY NOT NULL,
		ExperianNonLimitedResultCcjDetailsId INT,		
		Name NVARCHAR(75),				
		TradingIndicator NVARCHAR(1),		
		TradingIndicatorDesc NVARCHAR(25)
	)
END
GO

IF OBJECT_ID('ExperianNonLimitedResultPreviousSearches') IS NULL
BEGIN
	CREATE TABLE ExperianNonLimitedResultPreviousSearches (
		Id INT IDENTITY NOT NULL,
		ExperianNonLimitedResultId INT,
		PreviousSearchDate DATETIME,
		EnquiryType NVARCHAR(1),	
		EnquiryTypeDesc NVARCHAR(50),
		CreditRequired NVARCHAR(13)
	)
END
GO

IF OBJECT_ID('ExperianNonLimitedResultPaymentPerformanceDetails') IS NULL
BEGIN
	CREATE TABLE ExperianNonLimitedResultPaymentPerformanceDetails (
		Id INT IDENTITY NOT NULL,
		ExperianNonLimitedResultId INT,
		Code NVARCHAR(5),	
		DaysBeyondTerms INT
	)
END
GO


IF OBJECT_ID('ExperianNonLimitedResults') IS NULL
BEGIN
	CREATE TABLE ExperianNonLimitedResults (
		Id INT IDENTITY NOT NULL,
		CustomerId INT,
		RefNumber NVARCHAR(50),
		ServiceLogId INT,
		Created DATETIME,		
		BusinessName NVARCHAR(75),
		Address1 NVARCHAR(30),
		Address2 NVARCHAR(30),
		Address3 NVARCHAR(30),
		Address4 NVARCHAR(30),
		Address5 NVARCHAR(30),
		Postcode NVARCHAR(8),
		TelephoneNumber NVARCHAR(20),
		PrincipalActivities NVARCHAR(75),
		EarliestKnownDate DATETIME,
		DateOwnershipCommenced DATETIME,
		IncorporationDate DATETIME,
		DateOwnershipCeased DATETIME,
		LastUpdateDate DATETIME,		
		BankruptcyCountDuringOwnership INT,
		AgeOfMostRecentBankruptcyDuringOwnershipMonths INT,
		AssociatedBankruptcyCountDuringOwnership INT,
		AgeOfMostRecentAssociatedBankruptcyDuringOwnershipMonths INT,		
		AgeOfMostRecentJudgmentDuringOwnershipMonths INT,
		TotalJudgmentCountLast12Months INT,
		TotalJudgmentValueLast12Months INT,
		TotalJudgmentCountLast13To24Months INT,
		TotalJudgmentValueLast13To24Months INT,
		ValueOfMostRecentAssociatedJudgmentDuringOwnership INT,
		TotalAssociatedJudgmentCountLast12Months INT,
		TotalAssociatedJudgmentValueLast12Months INT,
		TotalAssociatedJudgmentCountLast13To24Months INT,
		TotalAssociatedJudgmentValueLast13To24Months INT,
		TotalJudgmentCountLast24Months INT,
		TotalAssociatedJudgmentCountLast24Months INT,
		TotalJudgmentCountValue24Months INT,
		TotalAssociatedJudgmentValueLast24Months INT,		
		SupplierName NVARCHAR(16),
		FraudCategory NVARCHAR(2),
		FraudCategoryDesc NVARCHAR(200),		
		NumberOfAccountsPlacedForCollection INT,
		ValueOfAccountsPlacedForCollection INT,
		NumberOfAccountsPlacedForCollectionLast2Years INT,
		AverageDaysBeyondTermsFor0To100 INT,
		AverageDaysBeyondTermsFor101To1000 INT,
		AverageDaysBeyondTermsFor1001To10000 INT,
		AverageDaysBeyondTermsForOver10000 INT,
		AverageDaysBeyondTermsForLast3MonthsOfDataReturned INT,
		AverageDaysBeyondTermsForLast6MonthsOfDataReturned INT,
		AverageDaysBeyondTermsForLast12MonthsOfDataReturned INT,
		CurrentAverageDebt INT,
		AverageDebtLast3Months INT,
		AverageDebtLast12Months INT,
		TelephoneNumberDN36 NVARCHAR(20),		
		RiskScore INT,		
		SearchType NVARCHAR(1),
		SearchTypeDesc NVARCHAR(25),
		CommercialDelphiScore INT,
		CreditRating NVARCHAR(8),
		CreditLimit NVARCHAR(8),
		ProbabilityOfDefaultScore DECIMAL(18,6),
		StabilityOdds NVARCHAR(10),
		RiskBand NVARCHAR(1),
		NumberOfProprietorsSearched INT,
		NumberOfProprietorsFound INT,		
		Errors NVARCHAR(MAX),		
		IsActive BIT
	)
END
GO
