SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('CreditSafeBaseData') IS NULL
BEGIN
	CREATE TABLE [CreditSafeBaseData] (
		[CreditSafeBaseDataID] BIGINT IDENTITY(1, 1) NOT NULL,
		[ServiceLogID] BIGINT NULL,
		[CompanyRefNum] NVARCHAR(10) NULL,
		[HasCreditSafeError] BIT NULL,
		[HasParsingError] BIT NULL,
		[Error] NVARCHAR(500) NULL,
		[InsertDate] DATETIME NULL,
		[Number] NVARCHAR(10) NULL,
		[Name] NVARCHAR(100) NULL,
		[Telephone] NVARCHAR(20) NULL,
		[TpsRegistered] BIT NULL,
		[Address1] NVARCHAR(100) NULL,
		[Address2] NVARCHAR(100) NULL,
		[Address3] NVARCHAR(100) NULL,
		[Address4] NVARCHAR(100) NULL,
		[Postcode] NVARCHAR(10) NULL,
		[SicCode] NVARCHAR(10) NULL,
		[SicDescription] NVARCHAR(500) NULL,
		[Website] NVARCHAR(100) NULL,
		[CompanyType] NVARCHAR(500) NULL,
		[AccountsType] NVARCHAR(100) NULL,
		[AnnualReturnDate] DATETIME NULL,
		[IncorporationDate] DATETIME NULL,
		[AccountsFilingDate] DATETIME NULL,
		[LatestAccountsDate] DATETIME NULL,
		[Quoted] NVARCHAR(10) NULL,
		[CompanyStatus] NVARCHAR(10) NULL,
		[CCJValues] INT NULL,
		[CCJNumbers] INT NULL,
		[CCJDateFrom] DATETIME NULL,
		[CCJDateTo] DATETIME NULL,
		[CCJNumberOfWrits] INT NULL,
		[Outstanding] INT NULL,
		[Satisfied] INT NULL,
		[ShareCapital] INT NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CreditSafeBaseData PRIMARY KEY ([CreditSafeBaseDataID]),
		CONSTRAINT FK_CreditSafeBaseData_ServiceLogID FOREIGN KEY ([ServiceLogID]) REFERENCES [MP_ServiceLog] ([Id])
	)
END
GO


IF OBJECT_ID('CreditSafeBaseData_SecondarySicCodes') IS NULL
BEGIN
	CREATE TABLE [CreditSafeBaseData_SecondarySicCodes] (
		[CreditSafeBaseData_SecondarySicCodesID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CreditSafeBaseDataID] BIGINT NULL,
		[Siccode] NVARCHAR(10) NULL,
		[SicDescription] NVARCHAR(500) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CreditSafeBaseData_SecondarySicCodes PRIMARY KEY ([CreditSafeBaseData_SecondarySicCodesID]),
		CONSTRAINT FK_CreditSafeBaseData_SecondarySicCodes_CreditSafeBaseDataID FOREIGN KEY ([CreditSafeBaseDataID]) REFERENCES [CreditSafeBaseData] ([CreditSafeBaseDataID])
	)
END
GO


IF OBJECT_ID('CreditSafeCCJDetails') IS NULL
BEGIN
	CREATE TABLE [CreditSafeCCJDetails] (
		[CreditSafeCCJDetailsID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CreditSafeBaseDataID] BIGINT NULL,
		[CaseNr] NVARCHAR(10) NULL,
		[CcjDate] DATETIME NULL,
		[Court] NVARCHAR(50) NULL,
		[CcjDatePaid] DATETIME NULL,
		[CcjStatus] NVARCHAR(10) NULL,
		[CcjAmount] INT NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CreditSafeCCJDetails PRIMARY KEY ([CreditSafeCCJDetailsID]),
		CONSTRAINT FK_CreditSafeCCJDetails_CreditSafeBaseDataID FOREIGN KEY ([CreditSafeBaseDataID]) REFERENCES [CreditSafeBaseData] ([CreditSafeBaseDataID])
	)
END
GO


IF OBJECT_ID('CreditSafeCreditLimits') IS NULL
BEGIN
	CREATE TABLE [CreditSafeCreditLimits] (
		[CreditSafeCreditLimitsID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CreditSafeBaseDataID] BIGINT NULL,
		[Limit] INT NULL,
		[Date] DATETIME NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CreditSafeCreditLimits PRIMARY KEY ([CreditSafeCreditLimitsID]),
		CONSTRAINT FK_CreditSafeCreditLimits_CreditSafeBaseDataID FOREIGN KEY ([CreditSafeBaseDataID]) REFERENCES [CreditSafeBaseData] ([CreditSafeBaseDataID])
	)
END
GO


IF OBJECT_ID('CreditSafeCreditRatings') IS NULL
BEGIN
	CREATE TABLE [CreditSafeCreditRatings] (
		[CreditSafeCreditRatingsID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CreditSafeBaseDataID] BIGINT NULL,
		[Date] DATETIME NULL,
		[Score] INT NULL,
		[Description] NVARCHAR(500) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CreditSafeCreditRatings PRIMARY KEY ([CreditSafeCreditRatingsID]),
		CONSTRAINT FK_CreditSafeCreditRatings_CreditSafeBaseDataID FOREIGN KEY ([CreditSafeBaseDataID]) REFERENCES [CreditSafeBaseData] ([CreditSafeBaseDataID])
	)
END
GO


IF OBJECT_ID('CreditSafeDirectors') IS NULL
BEGIN
	CREATE TABLE [CreditSafeDirectors] (
		[CreditSafeDirectorsID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CreditSafeBaseDataID] BIGINT NULL,
		[Title] NVARCHAR(10) NULL,
		[Name] NVARCHAR(100) NULL,
		[Address1] NVARCHAR(100) NULL,
		[Address2] NVARCHAR(100) NULL,
		[Address3] NVARCHAR(100) NULL,
		[Address4] NVARCHAR(100) NULL,
		[Address5] NVARCHAR(100) NULL,
		[Address6] NVARCHAR(100) NULL,
		[PostCode] NVARCHAR(10) NULL,
		[BirthDate] DATETIME NULL,
		[Nationality] NVARCHAR(100) NULL,
		[Honours] NVARCHAR(100) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CreditSafeDirectors PRIMARY KEY ([CreditSafeDirectorsID]),
		CONSTRAINT FK_CreditSafeDirectors_CreditSafeBaseDataID FOREIGN KEY ([CreditSafeBaseDataID]) REFERENCES [CreditSafeBaseData] ([CreditSafeBaseDataID])
	)
END
GO


IF OBJECT_ID('CreditSafeDirectors_Directorships') IS NULL
BEGIN
	CREATE TABLE [CreditSafeDirectors_Directorships] (
		[CreditSafeDirectors_DirectorshipsID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CreditSafeDirectorsID] BIGINT NULL,
		[CompanyNumber] NVARCHAR(100) NULL,
		[CompanyName] NVARCHAR(100) NULL,
		[CompanyStatus] NVARCHAR(100) NULL,
		[Function] NVARCHAR(100) NULL,
		[AppointedDate] DATETIME NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CreditSafeDirectors_Directorships PRIMARY KEY ([CreditSafeDirectors_DirectorshipsID]),
		CONSTRAINT FK_CreditSafeDirectors_Directorships_CreditSafeDirectorsID FOREIGN KEY ([CreditSafeDirectorsID]) REFERENCES [CreditSafeDirectors] ([CreditSafeDirectorsID])
	)
END
GO


IF OBJECT_ID('CreditSafeEventHistory') IS NULL
BEGIN
	CREATE TABLE [CreditSafeEventHistory] (
		[CreditSafeEventHistoryID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CreditSafeBaseDataID] BIGINT NULL,
		[Date] DATETIME NULL,
		[Text] NVARCHAR(500) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CreditSafeEventHistory PRIMARY KEY ([CreditSafeEventHistoryID]),
		CONSTRAINT FK_CreditSafeEventHistory_CreditSafeBaseDataID FOREIGN KEY ([CreditSafeBaseDataID]) REFERENCES [CreditSafeBaseData] ([CreditSafeBaseDataID])
	)
END
GO


IF OBJECT_ID('CreditSafeFinancial') IS NULL
BEGIN
	CREATE TABLE [CreditSafeFinancial] (
		[CreditSafeFinancialID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CreditSafeBaseDataID] BIGINT NULL,
		[DateFrom] DATETIME NULL,
		[DateTo] DATETIME NULL,
		[PeriodMonths] INT NULL,
		[Currency] NVARCHAR(10) NULL,
		[ConsolidatedAccounts] BIT NULL,
		[Turnover] INT NULL,
		[Export] INT NULL,
		[CostOfSales] INT NULL,
		[GrossProfit] INT NULL,
		[WagesSalaries] INT NULL,
		[DirectorEmoluments] INT NULL,
		[OperatingProfits] INT NULL,
		[Depreciation] INT NULL,
		[AuditFees] INT NULL,
		[InterestPayments] INT NULL,
		[Pretax] INT NULL,
		[Taxation] INT NULL,
		[PostTax] INT NULL,
		[DividendsPayable] INT NULL,
		[RetainedProfits] INT NULL,
		[TangibleAssets] INT NULL,
		[IntangibleAssets] INT NULL,
		[FixedAssets] INT NULL,
		[CurrentAssets] INT NULL,
		[TradeDebtors] INT NULL,
		[Stock] INT NULL,
		[Cash] INT NULL,
		[OtherCurrentAssets] INT NULL,
		[IncreaseInCash] INT NULL,
		[MiscellaneousCurrentAssets] INT NULL,
		[TotalAssets] INT NULL,
		[TotalCurrentLiabilities] INT NULL,
		[TradeCreditors] INT NULL,
		[OverDraft] INT NULL,
		[OtherShortTermFinance] INT NULL,
		[MiscellaneousCurrentLiabilities] INT NULL,
		[OtherLongTermFinance] INT NULL,
		[LongTermLiabilities] INT NULL,
		[OverDrafeLongTermLiabilities] INT NULL,
		[Liabilities] INT NULL,
		[NetAssets] INT NULL,
		[WorkingCapital] INT NULL,
		[PaidUpEquity] INT NULL,
		[ProfitLossReserve] INT NULL,
		[SundryReserves] INT NULL,
		[RevalutationReserve] INT NULL,
		[Reserves] INT NULL,
		[ShareholderFunds] INT NULL,
		[Networth] INT NULL,
		[NetCashFlowFromOperations] INT NULL,
		[NetCashFlowBeforeFinancing] INT NULL,
		[NetCashFlowFromFinancing] INT NULL,
		[ContingentLiability] BIT NULL,
		[CapitalEmployed] INT NULL,
		[Employees] INT NULL,
		[Auditors] NVARCHAR(100) NULL,
		[AuditQualification] NVARCHAR(100) NULL,
		[Bankers] NVARCHAR(100) NULL,
		[BankBranchCode] NVARCHAR(100) NULL,
		[PreTaxMargin] NVARCHAR(10) NULL,
		[CurrentRatio] NVARCHAR(10) NULL,
		[NetworkingCapital] NVARCHAR(10) NULL,
		[GearingRatio] NVARCHAR(10) NULL,
		[Equity] NVARCHAR(10) NULL,
		[CreditorDays] NVARCHAR(10) NULL,
		[DebtorDays] NVARCHAR(10) NULL,
		[Liquidity] NVARCHAR(10) NULL,
		[ReturnOnCapitalEmployed] NVARCHAR(10) NULL,
		[ReturnOnAssetsEmployed] NVARCHAR(10) NULL,
		[CurrentDebtRatio] NVARCHAR(10) NULL,
		[TotalDebtRatio] NVARCHAR(10) NULL,
		[StockTurnoverRatio] NVARCHAR(10) NULL,
		[ReturnOnNetAssetsEmployed] NVARCHAR(10) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CreditSafeFinancial PRIMARY KEY ([CreditSafeFinancialID]),
		CONSTRAINT FK_CreditSafeFinancial_CreditSafeBaseDataID FOREIGN KEY ([CreditSafeBaseDataID]) REFERENCES [CreditSafeBaseData] ([CreditSafeBaseDataID])
	)
END
GO


IF OBJECT_ID('CreditSafeIndustries') IS NULL
BEGIN
	CREATE TABLE [CreditSafeIndustries] (
		[CreditSafeIndustriesID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CreditSafeBaseDataID] BIGINT NULL,
		[Name] NVARCHAR(500) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CreditSafeIndustries PRIMARY KEY ([CreditSafeIndustriesID]),
		CONSTRAINT FK_CreditSafeIndustries_CreditSafeBaseDataID FOREIGN KEY ([CreditSafeBaseDataID]) REFERENCES [CreditSafeBaseData] ([CreditSafeBaseDataID])
	)
END
GO


IF OBJECT_ID('CreditSafeMortgages') IS NULL
BEGIN
	CREATE TABLE [CreditSafeMortgages] (
		[CreditSafeMortgagesID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CreditSafeBaseDataID] BIGINT NULL,
		[MortgageType] NVARCHAR(10) NULL,
		[CreateDate] DATETIME NULL,
		[RegisterDate] DATETIME NULL,
		[SatisfiedDate] DATETIME NULL,
		[Status] NVARCHAR(20) NULL,
		[AmountSecured] INT NULL,
		[Details] NVARCHAR(500) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CreditSafeMortgages PRIMARY KEY ([CreditSafeMortgagesID]),
		CONSTRAINT FK_CreditSafeMortgages_CreditSafeBaseDataID FOREIGN KEY ([CreditSafeBaseDataID]) REFERENCES [CreditSafeBaseData] ([CreditSafeBaseDataID])
	)
END
GO


IF OBJECT_ID('CreditSafeMortgages_PersonEntitled') IS NULL
BEGIN
	CREATE TABLE [CreditSafeMortgages_PersonEntitled] (
		[CreditSafeMortgages_PersonEntitledID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CreditSafeMortgagesID] BIGINT NULL,
		[Name] NVARCHAR(100) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CreditSafeMortgages_PersonEntitled PRIMARY KEY ([CreditSafeMortgages_PersonEntitledID]),
		CONSTRAINT FK_CreditSafeMortgages_PersonEntitled_CreditSafeMortgagesID FOREIGN KEY ([CreditSafeMortgagesID]) REFERENCES [CreditSafeMortgages] ([CreditSafeMortgagesID])
	)
END
GO


IF OBJECT_ID('CreditSafePreviousNames') IS NULL
BEGIN
	CREATE TABLE [CreditSafePreviousNames] (
		[CreditSafePreviousNamesID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CreditSafeBaseDataID] BIGINT NULL,
		[Name] NVARCHAR(100) NULL,
		[Date] DATETIME NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CreditSafePreviousNames PRIMARY KEY ([CreditSafePreviousNamesID]),
		CONSTRAINT FK_CreditSafePreviousNames_CreditSafeBaseDataID FOREIGN KEY ([CreditSafeBaseDataID]) REFERENCES [CreditSafeBaseData] ([CreditSafeBaseDataID])
	)
END
GO


IF OBJECT_ID('CreditSafeShareHolders') IS NULL
BEGIN
	CREATE TABLE [CreditSafeShareHolders] (
		[CreditSafeShareHoldersID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CreditSafeBaseDataID] BIGINT NULL,
		[Name] NVARCHAR(100) NULL,
		[Shares] NVARCHAR(250) NULL,
		[Currency] NVARCHAR(10) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CreditSafeShareHolders PRIMARY KEY ([CreditSafeShareHoldersID]),
		CONSTRAINT FK_CreditSafeShareHolders_CreditSafeBaseDataID FOREIGN KEY ([CreditSafeBaseDataID]) REFERENCES [CreditSafeBaseData] ([CreditSafeBaseDataID])
	)
END
GO


IF OBJECT_ID('CreditSafeStatusHistory') IS NULL
BEGIN
	CREATE TABLE [CreditSafeStatusHistory] (
		[CreditSafeStatusHistoryID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CreditSafeBaseDataID] BIGINT NULL,
		[date] DATETIME NULL,
		[text] NVARCHAR(500) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CreditSafeStatusHistory PRIMARY KEY ([CreditSafeStatusHistoryID]),
		CONSTRAINT FK_CreditSafeStatusHistory_CreditSafeBaseDataID FOREIGN KEY ([CreditSafeBaseDataID]) REFERENCES [CreditSafeBaseData] ([CreditSafeBaseDataID])
	)
END
GO


IF OBJECT_ID('CreditSafeTradingAddresses') IS NULL
BEGIN
	CREATE TABLE [CreditSafeTradingAddresses] (
		[CreditSafeTradingAddressesID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CreditSafeBaseDataID] BIGINT NULL,
		[Address1] NVARCHAR(100) NULL,
		[Address2] NVARCHAR(100) NULL,
		[Address3] NVARCHAR(100) NULL,
		[Address4] NVARCHAR(100) NULL,
		[PostCode] NVARCHAR(10) NULL,
		[Telephone] NVARCHAR(20) NULL,
		[TpsRegistered] BIT NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CreditSafeTradingAddresses PRIMARY KEY ([CreditSafeTradingAddressesID]),
		CONSTRAINT FK_CreditSafeTradingAddresses_CreditSafeBaseDataID FOREIGN KEY ([CreditSafeBaseDataID]) REFERENCES [CreditSafeBaseData] ([CreditSafeBaseDataID])
	)
END
GO


