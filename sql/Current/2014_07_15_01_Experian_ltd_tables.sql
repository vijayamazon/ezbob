SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('ExperianLtd') IS NULL
BEGIN
	CREATE TABLE ExperianLtd (
		ExperianLtdID BIGINT IDENTITY(1, 1) NOT NULL,
		ServiceLogID BIGINT NOT NULL,
		RegisteredNumber NVARCHAR(255) NULL,
		LegalStatus NVARCHAR(255) NULL,
		IncorporationDate DATETIME NULL,
		DissolutionDate DATETIME NULL,
		CompanyName NVARCHAR(255) NULL,
		OfficeAddress1 NVARCHAR(255) NULL,
		OfficeAddress2 NVARCHAR(255) NULL,
		OfficeAddress3 NVARCHAR(255) NULL,
		OfficeAddress4 NVARCHAR(255) NULL,
		OfficeAddressPostcode NVARCHAR(255) NULL,
		CommercialDelphiScore NVARCHAR(255) NULL,
		StabilityOdds NVARCHAR(255) NULL,
		CommercialDelphiBandText NVARCHAR(255) NULL,
		CommercialDelphiCreditLimit NVARCHAR(255) NULL,
		SameTradingAddressG NVARCHAR(255) NULL,
		LengthOf1992SICArea INT NULL,
		TradingPhoneNumber NVARCHAR(255) NULL,
		PrincipalActivities NVARCHAR(255) NULL,
		First1992SICCodeDescription NVARCHAR(255) NULL,
		BankSortcode NVARCHAR(255) NULL,
		BankName NVARCHAR(255) NULL,
		BankAddress1 NVARCHAR(255) NULL,
		BankAddress2 NVARCHAR(255) NULL,
		BankAddress3 NVARCHAR(255) NULL,
		BankAddress4 NVARCHAR(255) NULL,
		BankAddressPostcode NVARCHAR(255) NULL,
		RegisteredNumberOfTheCurrentUltimateParentCompany NVARCHAR(255) NULL,
		RegisteredNameOfTheCurrentUltimateParentCompany NVARCHAR(255) NULL,
		TotalNumberOfCurrentDirectors INT NULL,
		NumberOfCurrentDirectorshipsLessThan12Months INT NULL,
		NumberOfAppointmentsInTheLast12Months INT NULL,
		NumberOfResignationsInTheLast12Months INT NULL,
		AgeOfMostRecentCCJDecreeMonths INT NULL,
		NumberOfCCJsDuringLast12Months INT NULL,
		ValueOfCCJsDuringLast12Months DECIMAL(18, 6) NULL,
		NumberOfCCJsBetween13And24MonthsAgo INT NULL,
		ValueOfCCJsBetween13And24MonthsAgo DECIMAL(18, 6) NULL,
		CompanyAverageDBT3Months DECIMAL(18, 6) NULL,
		CompanyAverageDBT6Months DECIMAL(18, 6) NULL,
		CompanyAverageDBT12Months DECIMAL(18, 6) NULL,
		CompanyNumberOfDbt1000 DECIMAL(18, 6) NULL,
		CompanyNumberOfDbt10000 DECIMAL(18, 6) NULL,
		CompanyNumberOfDbt100000 DECIMAL(18, 6) NULL,
		CompanyNumberOfDbt100000Plus DECIMAL(18, 6) NULL,
		IndustryAverageDBT3Months DECIMAL(18, 6) NULL,
		IndustryAverageDBT6Months DECIMAL(18, 6) NULL,
		IndustryAverageDBT12Months DECIMAL(18, 6) NULL,
		IndustryNumberOfDbt1000 DECIMAL(18, 6) NULL,
		IndustryNumberOfDbt10000 DECIMAL(18, 6) NULL,
		IndustryNumberOfDbt100000 DECIMAL(18, 6) NULL,
		IndustryNumberOfDbt100000Plus DECIMAL(18, 6) NULL,
		CompanyPaymentPattern NVARCHAR(255) NULL,
		IndustryPaymentPattern NVARCHAR(255) NULL,
		SupplierPaymentPattern NVARCHAR(255) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_ExperianLtd PRIMARY KEY (ExperianLtdID),
		CONSTRAINT FK_ExperianLtd_ServiceLogID FOREIGN KEY (ServiceLogID) REFERENCES MP_ServiceLog(Id)
	)
END
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('ExperianLtdPrevCompanyNames') IS NULL
BEGIN
	CREATE TABLE ExperianLtdPrevCompanyNames (
		ExperianLtdPrevCompanyNamesID BIGINT IDENTITY(1, 1) NOT NULL,
		ExperianLtdID BIGINT NOT NULL,
		DateChanged DATETIME NULL,
		OfficeAddress1 NVARCHAR(255) NULL,
		OfficeAddress2 NVARCHAR(255) NULL,
		OfficeAddress3 NVARCHAR(255) NULL,
		OfficeAddress4 NVARCHAR(255) NULL,
		OfficeAddressPostcode NVARCHAR(255) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_ExperianLtdPrevCompanyNames PRIMARY KEY (ExperianLtdPrevCompanyNamesID),
		CONSTRAINT FK_ExperianLtdPrevCompanyNames_ExperianLtdID FOREIGN KEY (ExperianLtdID) REFERENCES ExperianLtd(ExperianLtdID)
	)
END
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('ExperianLtdShareholders') IS NULL
BEGIN
	CREATE TABLE ExperianLtdShareholders (
		ExperianLtdShareholdersID BIGINT IDENTITY(1, 1) NOT NULL,
		ExperianLtdID BIGINT NOT NULL,
		DescriptionOfShareholder NVARCHAR(255) NULL,
		DescriptionOfShareholding NVARCHAR(255) NULL,
		RegisteredNumberOfALimitedCompanyWhichIsAShareholder NVARCHAR(255) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_ExperianLtdShareholders PRIMARY KEY (ExperianLtdShareholdersID),
		CONSTRAINT FK_ExperianLtdShareholders_ExperianLtdID FOREIGN KEY (ExperianLtdID) REFERENCES ExperianLtd(ExperianLtdID)
	)
END
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('ExperianLtdDLB5') IS NULL
BEGIN
	CREATE TABLE ExperianLtdDLB5 (
		ExperianLtdDLB5ID BIGINT IDENTITY(1, 1) NOT NULL,
		ExperianLtdID BIGINT NOT NULL,
		RecordType NVARCHAR(255) NULL,
		IssueCompany NVARCHAR(255) NULL,
		CurrentpreviousIndicator NVARCHAR(255) NULL,
		EffectiveDate DATETIME NULL,
		ShareClassNumber NVARCHAR(255) NULL,
		ShareholdingNumber NVARCHAR(255) NULL,
		ShareholderNumber NVARCHAR(255) NULL,
		ShareholderType NVARCHAR(255) NULL,
		Prefix NVARCHAR(255) NULL,
		FirstName NVARCHAR(255) NULL,
		MidName1 NVARCHAR(255) NULL,
		LastName NVARCHAR(255) NULL,
		Suffix NVARCHAR(255) NULL,
		ShareholderQualifications NVARCHAR(255) NULL,
		Title NVARCHAR(255) NULL,
		ShareholderCompanyName NVARCHAR(255) NULL,
		KgenName NVARCHAR(255) NULL,
		ShareholderRegisteredNumber NVARCHAR(255) NULL,
		AddressLine1 NVARCHAR(255) NULL,
		AddressLine2 NVARCHAR(255) NULL,
		AddressLine3 NVARCHAR(255) NULL,
		Town NVARCHAR(255) NULL,
		County NVARCHAR(255) NULL,
		Postcode NVARCHAR(255) NULL,
		Country NVARCHAR(255) NULL,
		ShareholderPunaPcode NVARCHAR(255) NULL,
		ShareholderRMC NVARCHAR(255) NULL,
		SuppressionFlag NVARCHAR(255) NULL,
		NOCRefNumber NVARCHAR(255) NULL,
		LastUpdated DATETIME NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_ExperianLtdDLB5 PRIMARY KEY (ExperianLtdDLB5ID),
		CONSTRAINT FK_ExperianLtdDLB5_ExperianLtdID FOREIGN KEY (ExperianLtdID) REFERENCES ExperianLtd(ExperianLtdID)
	)
END
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('ExperianLtdDL72') IS NULL
BEGIN
	CREATE TABLE ExperianLtdDL72 (
		ExperianLtdDL72ID BIGINT IDENTITY(1, 1) NOT NULL,
		ExperianLtdID BIGINT NOT NULL,
		ForeignAddressFlag NVARCHAR(255) NULL,
		IsCompany NVARCHAR(255) NULL,
		Number NVARCHAR(255) NULL,
		LengthOfDirectorship INT NULL,
		DirectorsAgeYears INT NULL,
		NumberOfConvictions INT NULL,
		Prefix NVARCHAR(255) NULL,
		FirstName NVARCHAR(255) NULL,
		MidName1 NVARCHAR(255) NULL,
		MidName2 NVARCHAR(255) NULL,
		LastName NVARCHAR(255) NULL,
		Suffix NVARCHAR(255) NULL,
		Qualifications NVARCHAR(255) NULL,
		Title NVARCHAR(255) NULL,
		CompanyName NVARCHAR(255) NULL,
		CompanyNumber NVARCHAR(255) NULL,
		ShareInfo NVARCHAR(255) NULL,
		BirthDate DATETIME NULL,
		HouseName NVARCHAR(255) NULL,
		HouseNumber NVARCHAR(255) NULL,
		Street NVARCHAR(255) NULL,
		Town NVARCHAR(255) NULL,
		County NVARCHAR(255) NULL,
		Postcode NVARCHAR(255) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_ExperianLtdDL72 PRIMARY KEY (ExperianLtdDL72ID),
		CONSTRAINT FK_ExperianLtdDL72_ExperianLtdID FOREIGN KEY (ExperianLtdID) REFERENCES ExperianLtd(ExperianLtdID)
	)
END
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('ExperianLtdCreditSummary') IS NULL
BEGIN
	CREATE TABLE ExperianLtdCreditSummary (
		ExperianLtdCreditSummaryID BIGINT IDENTITY(1, 1) NOT NULL,
		ExperianLtdID BIGINT NOT NULL,
		CreditEventType NVARCHAR(255) NULL,
		DateOfMostRecentRecordForType DATETIME NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_ExperianLtdCreditSummary PRIMARY KEY (ExperianLtdCreditSummaryID),
		CONSTRAINT FK_ExperianLtdCreditSummary_ExperianLtdID FOREIGN KEY (ExperianLtdID) REFERENCES ExperianLtd(ExperianLtdID)
	)
END
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('ExperianLtdDL48') IS NULL
BEGIN
	CREATE TABLE ExperianLtdDL48 (
		ExperianLtdDL48ID BIGINT IDENTITY(1, 1) NOT NULL,
		ExperianLtdID BIGINT NOT NULL,
		FraudCategory NVARCHAR(255) NULL,
		SupplierName NVARCHAR(255) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_ExperianLtdDL48 PRIMARY KEY (ExperianLtdDL48ID),
		CONSTRAINT FK_ExperianLtdDL48_ExperianLtdID FOREIGN KEY (ExperianLtdID) REFERENCES ExperianLtd(ExperianLtdID)
	)
END
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('ExperianLtdDL52') IS NULL
BEGIN
	CREATE TABLE ExperianLtdDL52 (
		ExperianLtdDL52ID BIGINT IDENTITY(1, 1) NOT NULL,
		ExperianLtdID BIGINT NOT NULL,
		NoticeType NVARCHAR(255) NULL,
		DateOfNotice DATETIME NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_ExperianLtdDL52 PRIMARY KEY (ExperianLtdDL52ID),
		CONSTRAINT FK_ExperianLtdDL52_ExperianLtdID FOREIGN KEY (ExperianLtdID) REFERENCES ExperianLtd(ExperianLtdID)
	)
END
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('ExperianLtdDL68') IS NULL
BEGIN
	CREATE TABLE ExperianLtdDL68 (
		ExperianLtdDL68ID BIGINT IDENTITY(1, 1) NOT NULL,
		ExperianLtdID BIGINT NOT NULL,
		SubsidiaryRegisteredNumber NVARCHAR(255) NULL,
		SubsidiaryStatus NVARCHAR(255) NULL,
		SubsidiaryLegalStatus NVARCHAR(255) NULL,
		SubsidiaryName NVARCHAR(255) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_ExperianLtdDL68 PRIMARY KEY (ExperianLtdDL68ID),
		CONSTRAINT FK_ExperianLtdDL68_ExperianLtdID FOREIGN KEY (ExperianLtdID) REFERENCES ExperianLtd(ExperianLtdID)
	)
END
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('ExperianLtdDL97') IS NULL
BEGIN
	CREATE TABLE ExperianLtdDL97 (
		ExperianLtdDL97ID BIGINT IDENTITY(1, 1) NOT NULL,
		ExperianLtdID BIGINT NOT NULL,
		AccountState NVARCHAR(255) NULL,
		CompanyType INT NULL,
		AccountType INT NULL,
		DefaultDate DATETIME NULL,
		SettlementDate DATETIME NULL,
		CurrentBalance DECIMAL(18, 6) NULL,
		Status12 DECIMAL(18, 6) NULL,
		Status39 DECIMAL(18, 6) NULL,
		CAISLastUpdatedDate DATETIME NULL,
		AccountStatusLast12AccountStatuses NVARCHAR(255) NULL,
		AgreementNumber NVARCHAR(255) NULL,
		MonthsData NVARCHAR(255) NULL,
		DefaultBalance DECIMAL(18, 6) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_ExperianLtdDL97 PRIMARY KEY (ExperianLtdDL97ID),
		CONSTRAINT FK_ExperianLtdDL97_ExperianLtdID FOREIGN KEY (ExperianLtdID) REFERENCES ExperianLtd(ExperianLtdID)
	)
END
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('ExperianLtdDL99') IS NULL
BEGIN
	CREATE TABLE ExperianLtdDL99 (
		ExperianLtdDL99ID BIGINT IDENTITY(1, 1) NOT NULL,
		ExperianLtdID BIGINT NOT NULL,
		Date DATETIME NULL,
		CredDirLoans DECIMAL(18, 6) NULL,
		Debtors DECIMAL(18, 6) NULL,
		DebtorsDirLoans DECIMAL(18, 6) NULL,
		DebtorsGroupLoans DECIMAL(18, 6) NULL,
		InTngblAssets DECIMAL(18, 6) NULL,
		Inventories DECIMAL(18, 6) NULL,
		OnClDirLoans DECIMAL(18, 6) NULL,
		OtherDebtors DECIMAL(18, 6) NULL,
		PrepayAccRuals DECIMAL(18, 6) NULL,
		RetainedEarnings DECIMAL(18, 6) NULL,
		TngblAssets DECIMAL(18, 6) NULL,
		TotalCash DECIMAL(18, 6) NULL,
		TotalCurrLblts DECIMAL(18, 6) NULL,
		TotalNonCurr DECIMAL(18, 6) NULL,
		TotalShareFund DECIMAL(18, 6) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_ExperianLtdDL99 PRIMARY KEY (ExperianLtdDL99ID),
		CONSTRAINT FK_ExperianLtdDL99_ExperianLtdID FOREIGN KEY (ExperianLtdID) REFERENCES ExperianLtd(ExperianLtdID)
	)
END
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('ExperianLtdDLA2') IS NULL
BEGIN
	CREATE TABLE ExperianLtdDLA2 (
		ExperianLtdDLA2ID BIGINT IDENTITY(1, 1) NOT NULL,
		ExperianLtdID BIGINT NOT NULL,
		Date DATETIME NULL,
		NumberOfEmployees INT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_ExperianLtdDLA2 PRIMARY KEY (ExperianLtdDLA2ID),
		CONSTRAINT FK_ExperianLtdDLA2_ExperianLtdID FOREIGN KEY (ExperianLtdID) REFERENCES ExperianLtd(ExperianLtdID)
	)
END
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('ExperianLtdDL65') IS NULL
BEGIN
	CREATE TABLE ExperianLtdDL65 (
		ExperianLtdDL65ID BIGINT IDENTITY(1, 1) NOT NULL,
		ExperianLtdID BIGINT NOT NULL,
		ChargeNumber NVARCHAR(255) NULL,
		FormNumber NVARCHAR(255) NULL,
		CurrencyIndicator NVARCHAR(255) NULL,
		TotalAmountOfDebentureSecured NVARCHAR(255) NULL,
		ChargeType NVARCHAR(255) NULL,
		AmountSecured NVARCHAR(255) NULL,
		PropertyDetails NVARCHAR(255) NULL,
		ChargeeText NVARCHAR(255) NULL,
		RestrictingProvisions NVARCHAR(255) NULL,
		RegulatingProvisions NVARCHAR(255) NULL,
		AlterationsToTheOrder NVARCHAR(255) NULL,
		PropertyReleasedFromTheCharge NVARCHAR(255) NULL,
		AmountChargeIncreased NVARCHAR(255) NULL,
		CreationDate DATETIME NULL,
		DateFullySatisfied DATETIME NULL,
		FullySatisfiedIndicator NVARCHAR(255) NULL,
		NumberOfPartialSatisfactionDates INT NULL,
		NumberOfPartialSatisfactionDataItems INT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_ExperianLtdDL65 PRIMARY KEY (ExperianLtdDL65ID),
		CONSTRAINT FK_ExperianLtdDL65_ExperianLtdID FOREIGN KEY (ExperianLtdID) REFERENCES ExperianLtd(ExperianLtdID)
	)
END
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('ExperianLtdLenderDetails') IS NULL
BEGIN
	CREATE TABLE ExperianLtdLenderDetails (
		ExperianLtdLenderDetailsID BIGINT IDENTITY(1, 1) NOT NULL,
		DL65ID BIGINT NOT NULL,
		LenderName NVARCHAR(255) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_ExperianLtdLenderDetails PRIMARY KEY (ExperianLtdLenderDetailsID),
		CONSTRAINT FK_ExperianLtdLenderDetails_DL65ID FOREIGN KEY (DL65ID) REFERENCES ExperianLtdDL65(ExperianLtdDL65ID)
	)
END
GO


