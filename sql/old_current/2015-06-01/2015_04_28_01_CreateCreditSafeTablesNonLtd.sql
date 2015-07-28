SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('CreditSafeNonLtdBaseData') IS NULL
BEGIN
	CREATE TABLE [CreditSafeNonLtdBaseData] (
		[CreditSafeNonLtdBaseDataID] BIGINT IDENTITY(1, 1) NOT NULL,
		[ServiceLogID] BIGINT NULL,
		[EzbobCompanyID] NVARCHAR(10) NULL,
		[HasCreditSafeError] BIT NULL,
		[HasParsingError] BIT NULL,
		[Error] NVARCHAR(MAX) NULL,
		[InsertDate] DATETIME NULL,
		[Number] NVARCHAR(10) NULL,
		[Name] NVARCHAR(100) NULL,
		[Address1] NVARCHAR(100) NULL,
		[Address2] NVARCHAR(100) NULL,
		[Address3] NVARCHAR(100) NULL,
		[Address4] NVARCHAR(100) NULL,
		[PostCode] NVARCHAR(10) NULL,
		[MpsRegistered] BIT NULL,
		[AddressDate] DATETIME NULL,
		[AddressReason] NVARCHAR(100) NULL,
		[PremiseType] NVARCHAR(100) NULL,
		[Activities] NVARCHAR(100) NULL,
		[Employees] INT NULL,
		[Website] NVARCHAR(100) NULL,
		[Email] NVARCHAR(100) NULL,
		[MatchedCcjValue] INT NULL,
		[MatchedCcjNumber] INT NULL,
		[MatchedCcjDateFrom] DATETIME NULL,
		[MatchedCcjDateTo] DATETIME NULL,
		[PossibleCcjValue] INT NULL,
		[PossibleCcjNumber] INT NULL,
		[PossibleCcjDateFrom] DATETIME NULL,
		[PossibleCcjDateTo] DATETIME NULL,
		[ExecutiveName] NVARCHAR(100) NULL,
		[ExecutivePosition] NVARCHAR(100) NULL,
		[ExecutiveEmail] NVARCHAR(100) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CreditSafeNonLtdBaseData PRIMARY KEY ([CreditSafeNonLtdBaseDataID]),
		CONSTRAINT FK_CreditSafeNonLtdBaseData_ServiceLogID FOREIGN KEY ([ServiceLogID]) REFERENCES [MP_ServiceLog] ([Id])
	)
END
GO


IF OBJECT_ID('CreditSafeNonLtdBaseDataFax') IS NULL
BEGIN
	CREATE TABLE [CreditSafeNonLtdBaseDataFax] (
		[CreditSafeNonLtdBaseDataFaxID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CreditSafeNonLtdBaseDataID] BIGINT NULL,
		[Fax] NVARCHAR(20) NULL,
		[FpsRegistered] BIT NULL,
		[Main] BIT NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CreditSafeNonLtdBaseDataFax PRIMARY KEY ([CreditSafeNonLtdBaseDataFaxID]),
		CONSTRAINT FK_CreditSafeNonLtdBaseDataFax_CreditSafeNonLtdBaseDataID FOREIGN KEY ([CreditSafeNonLtdBaseDataID]) REFERENCES [CreditSafeNonLtdBaseData] ([CreditSafeNonLtdBaseDataID])
	)
END
GO


IF OBJECT_ID('CreditSafeNonLtdBaseDataTel') IS NULL
BEGIN
	CREATE TABLE [CreditSafeNonLtdBaseDataTel] (
		[CreditSafeNonLtdBaseDataTelID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CreditSafeNonLtdBaseDataID] BIGINT NULL,
		[Telephone] NVARCHAR(20) NULL,
		[TpsRegistered] BIT NULL,
		[Main] BIT NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CreditSafeNonLtdBaseDataTel PRIMARY KEY ([CreditSafeNonLtdBaseDataTelID]),
		CONSTRAINT FK_CreditSafeNonLtdBaseDataTel_CreditSafeNonLtdBaseDataID FOREIGN KEY ([CreditSafeNonLtdBaseDataID]) REFERENCES [CreditSafeNonLtdBaseData] ([CreditSafeNonLtdBaseDataID])
	)
END
GO


IF OBJECT_ID('CreditSafeNonLtdEvents') IS NULL
BEGIN
	CREATE TABLE [CreditSafeNonLtdEvents] (
		[CreditSafeNonLtdEventsID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CreditSafeNonLtdBaseDataID] BIGINT NULL,
		[Date] DATETIME NULL,
		[Text] NVARCHAR(250) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CreditSafeNonLtdEvents PRIMARY KEY ([CreditSafeNonLtdEventsID]),
		CONSTRAINT FK_CreditSafeNonLtdEvents_CreditSafeNonLtdBaseDataID FOREIGN KEY ([CreditSafeNonLtdBaseDataID]) REFERENCES [CreditSafeNonLtdBaseData] ([CreditSafeNonLtdBaseDataID])
	)
END
GO


IF OBJECT_ID('CreditSafeNonLtdLimits') IS NULL
BEGIN
	CREATE TABLE [CreditSafeNonLtdLimits] (
		[CreditSafeNonLtdLimitsID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CreditSafeNonLtdBaseDataID] BIGINT NULL,
		[Limit] INT NULL,
		[Date] DATETIME NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CreditSafeNonLtdLimits PRIMARY KEY ([CreditSafeNonLtdLimitsID]),
		CONSTRAINT FK_CreditSafeNonLtdLimits_CreditSafeNonLtdBaseDataID FOREIGN KEY ([CreditSafeNonLtdBaseDataID]) REFERENCES [CreditSafeNonLtdBaseData] ([CreditSafeNonLtdBaseDataID])
	)
END
GO


IF OBJECT_ID('CreditSafeNonLtdMatchedCCJ') IS NULL
BEGIN
	CREATE TABLE [CreditSafeNonLtdMatchedCCJ] (
		[CreditSafeNonLtdMatchedCCJID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CreditSafeNonLtdBaseDataID] BIGINT NULL,
		[CaseNr] NVARCHAR(10) NULL,
		[CcjDate] DATETIME NULL,
		[CcjDatePaid] DATETIME NULL,
		[Court] NVARCHAR(50) NULL,
		[CcjStatus] NVARCHAR(10) NULL,
		[CcjAmount] INT NULL,
		[Against] NVARCHAR(100) NULL,
		[Address] NVARCHAR(100) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CreditSafeNonLtdMatchedCCJ PRIMARY KEY ([CreditSafeNonLtdMatchedCCJID]),
		CONSTRAINT FK_CreditSafeNonLtdMatchedCCJ_CreditSafeNonLtdBaseDataID FOREIGN KEY ([CreditSafeNonLtdBaseDataID]) REFERENCES [CreditSafeNonLtdBaseData] ([CreditSafeNonLtdBaseDataID])
	)
END
GO


IF OBJECT_ID('CreditSafeNonLtdPossibleCCJ') IS NULL
BEGIN
	CREATE TABLE [CreditSafeNonLtdPossibleCCJ] (
		[CreditSafeNonLtdPossibleCCJID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CreditSafeNonLtdBaseDataID] BIGINT NULL,
		[CaseNr] NVARCHAR(10) NULL,
		[CcjDate] DATETIME NULL,
		[CcjDatePaid] DATETIME NULL,
		[Court] NVARCHAR(50) NULL,
		[CcjStatus] NVARCHAR(10) NULL,
		[CcjAmount] INT NULL,
		[Against] NVARCHAR(100) NULL,
		[Address] NVARCHAR(100) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CreditSafeNonLtdPossibleCCJ PRIMARY KEY ([CreditSafeNonLtdPossibleCCJID]),
		CONSTRAINT FK_CreditSafeNonLtdPossibleCCJ_CreditSafeNonLtdBaseDataID FOREIGN KEY ([CreditSafeNonLtdBaseDataID]) REFERENCES [CreditSafeNonLtdBaseData] ([CreditSafeNonLtdBaseDataID])
	)
END
GO


IF OBJECT_ID('CreditSafeNonLtdRatings') IS NULL
BEGIN
	CREATE TABLE [CreditSafeNonLtdRatings] (
		[CreditSafeNonLtdRatingsID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CreditSafeNonLtdBaseDataID] BIGINT NULL,
		[Date] DATETIME NULL,
		[Score] INT NULL,
		[Description] NVARCHAR(100) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CreditSafeNonLtdRatings PRIMARY KEY ([CreditSafeNonLtdRatingsID]),
		CONSTRAINT FK_CreditSafeNonLtdRatings_CreditSafeNonLtdBaseDataID FOREIGN KEY ([CreditSafeNonLtdBaseDataID]) REFERENCES [CreditSafeNonLtdBaseData] ([CreditSafeNonLtdBaseDataID])
	)
END
GO


