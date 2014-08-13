SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('ExperianConsumerData') IS NULL
BEGIN
	CREATE TABLE ExperianConsumerData (
		Id BIGINT IDENTITY(1, 1) NOT NULL,
		ServiceLogId BIGINT NULL,
		CustomerId INT NULL,
		DirectorId INT NULL,
		Error NVARCHAR(255) NULL,
		HasParsingError BIT NOT NULL,
		HasExperianError BIT NOT NULL,
		BureauScore INT NULL,
		CII INT NULL,
		CreditCardBalances INT NULL,
		ActiveCaisBalanceExcMortgages INT NULL,
		NumCreditCards INT NULL,
		CreditLimitUtilisation INT NULL,
		CreditCardOverLimit INT NULL,
		PersonalLoanStatus NVARCHAR(255) NULL,
		WorstStatus NVARCHAR(255) NULL,
		WorstCurrentStatus NVARCHAR(255) NULL,
		WorstHistoricalStatus NVARCHAR(255) NULL,
		TotalAccountBalances INT NULL,
		NumAccounts INT NULL,
		NumCCJs INT NULL,
		CCJLast2Years INT NULL,
		TotalCCJValue1 INT NULL,
		TotalCCJValue2 INT NULL,
		EnquiriesLast6Months INT NULL,
		EnquiriesLast3Months INT NULL,
		MortgageBalance INT NULL,
		CaisDOB DATETIME NULL,
		CreditCommitmentsRevolving INT NULL,
		CreditCommitmentsNonRevolving INT NULL,
		MortgagePayments INT NULL,
		Bankruptcy BIT NOT NULL,
		OtherBankruptcy BIT NOT NULL,
		CAISDefaults INT NULL,
		BadDebt NVARCHAR(255) NULL,
		NOCsOnCCJ BIT NOT NULL,
		NOCsOnCAIS BIT NOT NULL,
		NOCAndNOD BIT NOT NULL,
		SatisfiedJudgement BIT NOT NULL,
		CAISSpecialInstructionFlag NVARCHAR(255) NULL,
		CONSTRAINT PK_ExperianConsumerData PRIMARY KEY (Id),
		CONSTRAINT FK_ExperianConsumerData_ServiceLog FOREIGN KEY (ServiceLogId) REFERENCES MP_ServiceLog(Id)
	)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_ExperianConsumerData_ServiceLog' AND object_id = OBJECT_ID('ExperianConsumerData'))
	CREATE INDEX IX_ExperianConsumerData_ServiceLog ON ExperianConsumerData(ServiceLogId);
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('ExperianConsumerDataApplicant') IS NULL
BEGIN
	CREATE TABLE ExperianConsumerDataApplicant (
		Id BIGINT IDENTITY(1, 1) NOT NULL,
		ExperianConsumerDataId BIGINT NULL,
		ApplicantIdentifier INT NULL,
		Title NVARCHAR(255) NULL,
		Forename NVARCHAR(255) NULL,
		MiddleName NVARCHAR(255) NULL,
		Surname NVARCHAR(255) NULL,
		Suffix NVARCHAR(255) NULL,
		DateOfBirth DATETIME NULL,
		Gender NVARCHAR(255) NULL,
		CONSTRAINT PK_ExperianConsumerDataApplicant PRIMARY KEY (Id),
		CONSTRAINT FK_ExperianConsumerDataApplicant_ExperianConsumerData FOREIGN KEY (ExperianConsumerDataId) REFERENCES ExperianConsumerData(Id)
	)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_ExperianConsumerDataApplicant_ExperianConsumerData' AND object_id = OBJECT_ID('ExperianConsumerDataApplicant'))
	CREATE INDEX IX_ExperianConsumerDataApplicant_ExperianConsumerData ON ExperianConsumerDataApplicant(ExperianConsumerDataId);
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('ExperianConsumerDataCais') IS NULL
BEGIN
	CREATE TABLE ExperianConsumerDataCais (
		Id BIGINT IDENTITY(1, 1) NOT NULL,
		ExperianConsumerDataId BIGINT NULL,
		CAISAccStartDate DATETIME NULL,
		SettlementDate DATETIME NULL,
		LastUpdatedDate DATETIME NULL,
		MatchTo INT NULL,
		CreditLimit INT NULL,
		Balance INT NULL,
		CurrentDefBalance INT NULL,
		DelinquentBalance INT NULL,
		AccountStatusCodes NVARCHAR(255) NULL,
		Status1To2 NVARCHAR(255) NULL,
		StatusTo3 NVARCHAR(255) NULL,
		NumOfMonthsHistory INT NULL,
		WorstStatus NVARCHAR(255) NULL,
		AccountStatus NVARCHAR(255) NULL,
		AccountType NVARCHAR(255) NULL,
		CompanyType NVARCHAR(255) NULL,
		RepaymentPeriod INT NULL,
		Payment INT NULL,
		NumAccountBalances INT NULL,
		NumCardHistories INT NULL,
		CONSTRAINT PK_ExperianConsumerDataCais PRIMARY KEY (Id),
		CONSTRAINT FK_ExperianConsumerDataCais_ExperianConsumerData FOREIGN KEY (ExperianConsumerDataId) REFERENCES ExperianConsumerData(Id)
	)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_ExperianConsumerDataCais_ExperianConsumerData' AND object_id = OBJECT_ID('ExperianConsumerDataCais'))
	CREATE INDEX IX_ExperianConsumerDataCais_ExperianConsumerData ON ExperianConsumerDataCais(ExperianConsumerDataId);
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('ExperianConsumerDataCaisBalance') IS NULL
BEGIN
	CREATE TABLE ExperianConsumerDataCaisBalance (
		Id BIGINT IDENTITY(1, 1) NOT NULL,
		ExperianConsumerDataCaisId BIGINT NULL,
		AccountBalance INT NULL,
		Status NVARCHAR(255) NULL,
		CONSTRAINT PK_ExperianConsumerDataCaisBalance PRIMARY KEY (Id),
		CONSTRAINT FK_ExperianConsumerDataCaisBalance_ExperianConsumerDataCais FOREIGN KEY (ExperianConsumerDataCaisId) REFERENCES ExperianConsumerDataCais(Id)
	)
END
GO


SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('ExperianConsumerDataCaisCardHistory') IS NULL
BEGIN
	CREATE TABLE ExperianConsumerDataCaisCardHistory (
		Id BIGINT IDENTITY(1, 1) NOT NULL,
		ExperianConsumerDataCaisId BIGINT NULL,
		PrevStatementBal INT NULL,
		PromotionalRate NVARCHAR(255) NULL,
		PaymentAmount INT NULL,
		NumCashAdvances INT NULL,
		CashAdvanceAmount INT NULL,
		PaymentCode NVARCHAR(255) NULL,
		CONSTRAINT PK_ExperianConsumerDataCaisCardHistory PRIMARY KEY (Id),
		CONSTRAINT FK_ExperianConsumerDataCaisCardHistory_ExperianConsumerDataCais FOREIGN KEY (ExperianConsumerDataCaisId) REFERENCES ExperianConsumerDataCais(Id)
	)
END
GO


SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('ExperianConsumerDataLocation') IS NULL
BEGIN
	CREATE TABLE ExperianConsumerDataLocation (
		Id BIGINT IDENTITY(1, 1) NOT NULL,
		ExperianConsumerDataId BIGINT NULL,
		LocationIdentifier INT NULL,
		Flat NVARCHAR(255) NULL,
		HouseName NVARCHAR(255) NULL,
		HouseNumber NVARCHAR(255) NULL,
		Street NVARCHAR(255) NULL,
		Street2 NVARCHAR(255) NULL,
		District NVARCHAR(255) NULL,
		District2 NVARCHAR(255) NULL,
		PostTown NVARCHAR(255) NULL,
		County NVARCHAR(255) NULL,
		Postcode NVARCHAR(255) NULL,
		POBox NVARCHAR(255) NULL,
		Country NVARCHAR(255) NULL,
		SharedLetterbox NVARCHAR(255) NULL,
		FormattedLocation NVARCHAR(255) NULL,
		LocationCode NVARCHAR(255) NULL,
		TimeAtYears NVARCHAR(255) NULL,
		TimeAtMonths NVARCHAR(255) NULL,
		CONSTRAINT PK_ExperianConsumerDataLocation PRIMARY KEY (Id),
		CONSTRAINT FK_ExperianConsumerDataLocation_ExperianConsumerData FOREIGN KEY (ExperianConsumerDataId) REFERENCES ExperianConsumerData(Id)
	)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_ExperianConsumerDataLocation_ExperianConsumerData' AND object_id = OBJECT_ID('ExperianConsumerDataLocation'))
	CREATE INDEX IX_ExperianConsumerDataLocation_ExperianConsumerData ON ExperianConsumerDataLocation(ExperianConsumerDataId);
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('ExperianConsumerDataNoc') IS NULL
BEGIN
	CREATE TABLE ExperianConsumerDataNoc (
		Id BIGINT IDENTITY(1, 1) NOT NULL,
		ExperianConsumerDataId BIGINT NULL,
		Reference NVARCHAR(255) NULL,
		TextLine NVARCHAR(255) NULL,
		CONSTRAINT PK_ExperianConsumerDataNoc PRIMARY KEY (Id),
		CONSTRAINT FK_ExperianConsumerDataNoc_ExperianConsumerData FOREIGN KEY (ExperianConsumerDataId) REFERENCES ExperianConsumerData(Id)
	)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_ExperianConsumerDataNoc_ExperianConsumerData' AND object_id = OBJECT_ID('ExperianConsumerDataNoc'))
	CREATE INDEX IX_ExperianConsumerDataNoc_ExperianConsumerData ON ExperianConsumerDataNoc(ExperianConsumerDataId);
GO


SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('ExperianConsumerDataResidency') IS NULL
BEGIN
	CREATE TABLE ExperianConsumerDataResidency (
		Id BIGINT IDENTITY(1, 1) NOT NULL,
		ExperianConsumerDataId BIGINT NULL,
		ApplicantIdentifier INT NULL,
		LocationIdentifier INT NULL,
		LocationCode NVARCHAR(255) NULL,
		ResidencyDateFrom DATETIME NULL,
		ResidencyDateTo DATETIME NULL,
		TimeAtYears NVARCHAR(255) NULL,
		TimeAtMonths NVARCHAR(255) NULL,
		CONSTRAINT PK_ExperianConsumerDataResidency PRIMARY KEY (Id),
		CONSTRAINT FK_ExperianConsumerDataResidency_ExperianConsumerData FOREIGN KEY (ExperianConsumerDataId) REFERENCES ExperianConsumerData(Id)
	)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_ExperianConsumerDataResidency_ExperianConsumerData' AND object_id = OBJECT_ID('ExperianConsumerDataResidency'))
	CREATE INDEX IX_ExperianConsumerDataResidency_ExperianConsumerData ON ExperianConsumerDataResidency(ExperianConsumerDataId);
GO

