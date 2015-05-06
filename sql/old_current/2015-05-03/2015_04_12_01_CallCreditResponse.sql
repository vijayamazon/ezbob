SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('CallCredit') IS NULL
BEGIN
	CREATE TABLE [CallCredit] (
		[CallCreditID] BIGINT IDENTITY(1, 1) NOT NULL,
		[MP_ServiceLogId] BIGINT NULL,
		[InsertDate] DATETIME NOT NULL,
		[CustomerId] INT NULL,
		[DirectorId] INT NULL,
		[Error] NVARCHAR(4000) NULL,
		[HasParsingError] BIT NOT NULL,
		[HasCallCreditError] BIT NOT NULL,
		[LinkType] INT NULL,
		[ReportSearchID] NVARCHAR(38) NULL,
		[PayLoadData] NVARCHAR(4000) NULL,
		[YourReference] NVARCHAR(50) NULL,
		[Token] NVARCHAR(64) NULL,
		[SchemaVersionCR] NVARCHAR(5) NULL,
		[DataSetsCR] INT NULL,
		[Score] BIT NULL,
		[Purpose] NVARCHAR(10) NULL,
		[CreditType] NVARCHAR(10) NULL,
		[BalorLim] INT NULL,
		[Term] NVARCHAR(10) NULL,
		[Transient] BIT NULL,
		[AutoSearch] BIT NULL,
		[AutoSearchMaximum] INT NULL,
		[UniqueSearchID] NVARCHAR(38) NULL,
		[CastInfo] NVARCHAR(1000) NULL,
		[PSTV] INT NULL,
		[LS] INT NULL,
		[SearchDate] DATETIME NULL,
		[SchemaVersionLR] NVARCHAR(5) NULL,
		[DataSetsLR] INT NULL,
		[OrigSrchLRID] NVARCHAR(38) NULL,
		[NavLinkID] NVARCHAR(38) NULL,
		[SchemaVersionSR] NVARCHAR(5) NULL,
		[DataSetsSR] INT NULL,
		[OrigSrchSRID] NVARCHAR(38) NULL,
		[Dob] DATETIME NULL,
		[Hho] BIT NULL,
		[TpOptOut] BIT NULL,
		[CustomerStatus] NVARCHAR(10) NULL,
		[MaritalStatus] NVARCHAR(10) NULL,
		[TotalDependents] INT NULL,
		[LanguageVerbal] NVARCHAR(10) NULL,
		[Type1] NVARCHAR(10) NULL,
		[Type2] NVARCHAR(10) NULL,
		[Type3] NVARCHAR(10) NULL,
		[Type4] NVARCHAR(10) NULL,
		[AccommodationType] NVARCHAR(10) NULL,
		[PropertyValue] INT NULL,
		[MortgageBalance] INT NULL,
		[MonthlyRental] INT NULL,
		[ResidentialStatus] NVARCHAR(10) NULL,
		[Occupation] NVARCHAR(10) NULL,
		[EmploymentStatus] NVARCHAR(10) NULL,
		[ExpiryDate] DATETIME NULL,
		[EmploymentRecency] NVARCHAR(10) NULL,
		[EmployerCategory] NVARCHAR(10) NULL,
		[TimeAtCurrentEmployer] NVARCHAR(15) NULL,
		[SortCode] NVARCHAR(6) NULL,
		[AccountNumber] NVARCHAR(20) NULL,
		[TimeAtBank] NVARCHAR(15) NULL,
		[PaymentMethod] NVARCHAR(10) NULL,
		[FinanceType] NVARCHAR(10) NULL,
		[TotalDebitCards] INT NULL,
		[TotalCreditCards] INT NULL,
		[MonthlyUnsecuredAmount] INT NULL,
		[AmountPr] INT NULL,
		[TypePr] NVARCHAR(10) NULL,
		[PaymentMethodPr] NVARCHAR(10) NULL,
		[FrequencyPr] NVARCHAR(10) NULL,
		[AmountAd] INT NULL,
		[TypeAd] NVARCHAR(10) NULL,
		[PaymentMethodAd] NVARCHAR(10) NULL,
		[FrequencyAd] NVARCHAR(10) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CallCredit PRIMARY KEY ([CallCreditID]),
		CONSTRAINT FK_CallCredit_MP_ServiceLogId FOREIGN KEY ([MP_ServiceLogId]) REFERENCES [MP_ServiceLog] ([ID]),
	)
END
GO


IF OBJECT_ID('CallCreditEmail') IS NULL
BEGIN
	CREATE TABLE [CallCreditEmail] (
		[CallCreditEmailID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CallCreditID] BIGINT NULL,
		[EmailType] NVARCHAR(10) NULL,
		[EmailAddress] NVARCHAR(100) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CallCreditEmail PRIMARY KEY ([CallCreditEmailID]),
		CONSTRAINT FK_CallCreditEmail_CallCreditID FOREIGN KEY ([CallCreditID]) REFERENCES [CallCredit] ([CallCreditID]),
	)
END
GO


IF OBJECT_ID('CallCreditTelephone') IS NULL
BEGIN
	CREATE TABLE [CallCreditTelephone] (
		[CallCreditTelephoneID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CallCreditID] BIGINT NULL,
		[TelephoneType] NVARCHAR(10) NULL,
		[STD] NVARCHAR(5) NULL,
		[PhoneNumber] NVARCHAR(11) NULL,
		[Extension] NVARCHAR(5) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CallCreditTelephone PRIMARY KEY ([CallCreditTelephoneID]),
		CONSTRAINT FK_CallCreditTelephone_CallCreditID FOREIGN KEY ([CallCreditID]) REFERENCES [CallCredit] ([CallCreditID]),
	)
END
GO


IF OBJECT_ID('CallCreditAmendments') IS NULL
BEGIN
	CREATE TABLE [CallCreditAmendments] (
		[CallCreditAmendmentsID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CallCreditID] BIGINT NULL,
		[AmendmentName] NVARCHAR(20) NULL,
		[AmendmentType] NVARCHAR(6) NULL,
		[Balorlim] INT NULL,
		[Term] NVARCHAR(15) NULL,
		[AbodeNo] NVARCHAR(30) NULL,
		[BuildingNo] NVARCHAR(12) NULL,
		[BuildingName] NVARCHAR(50) NULL,
		[Street1] NVARCHAR(50) NULL,
		[Street2] NVARCHAR(50) NULL,
		[Sublocality] NVARCHAR(35) NULL,
		[Locality] NVARCHAR(35) NULL,
		[PostTown] NVARCHAR(25) NULL,
		[PostCode] NVARCHAR(8) NULL,
		[StartDate] DATETIME NULL,
		[EndDate] DATETIME NULL,
		[Duration] NVARCHAR(30) NULL,
		[Title] NVARCHAR(30) NULL,
		[Forename] NVARCHAR(30) NULL,
		[OtherNames] NVARCHAR(40) NULL,
		[SurName] NVARCHAR(30) NULL,
		[Suffix] NVARCHAR(30) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CallCreditAmendments PRIMARY KEY ([CallCreditAmendmentsID]),
		CONSTRAINT FK_CallCreditAmendments_CallCreditID FOREIGN KEY ([CallCreditID]) REFERENCES [CallCredit] ([CallCreditID]),
	)
END
GO


IF OBJECT_ID('CallCreditApplicantAddresses') IS NULL
BEGIN
	CREATE TABLE [CallCreditApplicantAddresses] (
		[CallCreditApplicantAddressesID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CallCreditID] BIGINT NULL,
		[AbodeNo] NVARCHAR(30) NULL,
		[BuildingNo] NVARCHAR(12) NULL,
		[BuildingName] NVARCHAR(50) NULL,
		[Street1] NVARCHAR(50) NULL,
		[Street2] NVARCHAR(50) NULL,
		[SubLocality] NVARCHAR(35) NULL,
		[Locality] NVARCHAR(35) NULL,
		[PostTown] NVARCHAR(25) NULL,
		[PostCode] NVARCHAR(8) NULL,
		[StartDate] DATETIME NULL,
		[EndDate] DATETIME NULL,
		[Duration] NVARCHAR(15) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CallCreditApplicantAddresses PRIMARY KEY ([CallCreditApplicantAddressesID]),
		CONSTRAINT FK_CallCreditApplicantAddresses_CallCreditID FOREIGN KEY ([CallCreditID]) REFERENCES [CallCredit] ([CallCreditID]),
	)
END
GO


IF OBJECT_ID('CallCreditApplicantNames') IS NULL
BEGIN
	CREATE TABLE [CallCreditApplicantNames] (
		[CallCreditApplicantNamesID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CallCreditID] BIGINT NULL,
		[Title] NVARCHAR(30) NULL,
		[Forename] NVARCHAR(30) NULL,
		[OtherNames] NVARCHAR(40) NULL,
		[Surname] NVARCHAR(30) NULL,
		[Suffix] NVARCHAR(30) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CallCreditApplicantNames PRIMARY KEY ([CallCreditApplicantNamesID]),
		CONSTRAINT FK_CallCreditApplicantNames_CallCreditID FOREIGN KEY ([CallCreditID]) REFERENCES [CallCredit] ([CallCreditID]),
	)
END
GO


IF OBJECT_ID('CallCreditData') IS NULL
BEGIN
	CREATE TABLE [CallCreditData] (
		[CallCreditDataID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CallCreditID] BIGINT NULL,
		[OiaID] INT NULL,
		[ReportType] NVARCHAR(1) NULL,
		[TpOptOut] BIT NULL,
		[AutoSearchMaxExceeded] BIT NULL,
		[AgeFlag] INT NULL,
		[ReporTitle] NVARCHAR(500) NULL,
		[CurrentInsolvment] BIT NULL,
		[Restricted] BIT NULL,
		[TotalDischarged] INT NULL,
		[TotalMinPayments12Month] INT NULL,
		[TotalMinPayments36Month] INT NULL,
		[TotalValueCashAdvances12Month] INT NULL,
		[TotalValueCashAdvances36Month] INT NULL,
		[TotalCifas] INT NULL,
		[ImpairedCredit] BIT NULL,
		[Secured] BIT NULL,
		[Unsecured] BIT NULL,
		[Judgment] BIT NULL,
		[Iva] BIT NULL,
		[Boss] BIT NULL,
		[BalanceLimitRatioVolve] BIGINT NOT NULL,
		[TotalBalancesActive] BIGINT NOT NULL,
		[TotalBalancesLoans] BIGINT NOT NULL,
		[TotalBalancesMortgage] BIGINT NOT NULL,
		[TotalBalancesRevolve] BIGINT NOT NULL,
		[TotalLimitsRevolve] BIGINT NOT NULL,
		[Total] INT NULL,
		[TotalActive] INT NULL,
		[Total36m] INT NULL,
		[TotalSatisfied] INT NULL,
		[TotalActiveAmount] INT NULL,
		[TotalSatisfiedAmount] INT NULL,
		[TotalUndecAddresses] INT NULL,
		[TotalUndecAddressesSearched] INT NULL,
		[TotalUndecAddressesUnsearched] INT NULL,
		[TotalUndecAliases] INT NULL,
		[TotalUndecAssociates] INT NULL,
		[HasUpdates] BIT NULL,
		[TotalHomeCreditSearches3Months] INT NULL,
		[TotalSearches3Months] INT NULL,
		[TotalSearches12Months] INT NULL,
		[TotalAccounts] INT NULL,
		[TotalActiveAccs] INT NULL,
		[TotalSettledAccs] INT NULL,
		[TotalOpened6Month] INT NULL,
		[WorstPayStatus12Month] NVARCHAR(10) NULL,
		[WorstPayStatus36Month] NVARCHAR(10) NULL,
		[TotalDelinqs12Month] INT NULL,
		[TotalDefaults12Month] INT NULL,
		[TotalDefaults36Month] INT NULL,
		[MessageCode] INT NULL,
		[PafValid] BIT NULL,
		[RollingRoll] BIT NULL,
		[AlertDecision] INT NULL,
		[AlertReview] INT NULL,
		[Hho] INT NULL,
		[NocFlag] BIT NULL,
		[TotalDisputes] INT NULL,
		[PersonName] NVARCHAR(164) NULL,
		[Dob] DATETIME NULL,
		[CurrentAddressP] BIT NULL,
		[UnDeclaredAddressTypeP] INT NULL,
		[AddressValueP] NVARCHAR(440) NULL,
		[number] NVARCHAR(8) NULL,
		[CompanyName] NVARCHAR(70) NULL,
		[CurrentAddressC] BIT NULL,
		[UnDeclaredAddressTypeC] INT NULL,
		[ValueC] NVARCHAR(440) NULL,
		[MemberNumber] INT NULL,
		[CaseReferenceNo] NVARCHAR(6) NULL,
		[NameDetails] NVARCHAR(100) NULL,
		[ProductCode] NVARCHAR(10) NULL,
		[FraudCategory] NVARCHAR(10) NULL,
		[ProductDesc] NVARCHAR(150) NULL,
		[FraudDesc] NVARCHAR(50) NULL,
		[InputDate] DATETIME NULL,
		[ExpiryDate] DATETIME NULL,
		[TransactionType] NVARCHAR(10) NULL,
		[CameoUk] NVARCHAR(2) NULL,
		[CameoInvestor] NVARCHAR(2) NULL,
		[CameoIncome] NVARCHAR(2) NULL,
		[CameoUnemployment] NVARCHAR(2) NULL,
		[CameoProperty] NVARCHAR(2) NULL,
		[CameoFinance] NVARCHAR(2) NULL,
		[CameoUkFam] NVARCHAR(2) NULL,
		[Ind_adult1] INT NULL,
		[Adult_1] INT NULL,
		[Adults_2] INT NULL,
		[Adult_3pl] INT NULL,
		[Age0_17] INT NULL,
		[Age18_24] INT NULL,
		[Age25_34] INT NULL,
		[Age35_44] INT NULL,
		[Age45_54] INT NULL,
		[Age55_64] INT NULL,
		[Age65_74] INT NULL,
		[Age75pl] INT NULL,
		[Unem_prob] FLOAT NULL,
		[Unem_index] INT NULL,
		[Wk_fem_ind] INT NULL,
		[Stu_ind] INT NULL,
		[Sick_ind] INT NULL,
		[Degree_ind] INT NULL,
		[Ab_ind] INT NULL,
		[C1_ind] INT NULL,
		[C2_ind] INT NULL,
		[De_ind] INT NULL,
		[Cameoukhsg] NVARCHAR(2) NULL,
		[Cameoukten] NVARCHAR(2) NULL,
		[Natprice] INT NULL,
		[Regprice] INT NULL,
		[D_index] INT NULL,
		[D_r_index] INT NULL,
		[S_index] INT NULL,
		[S_r_index] INT NULL,
		[T_index] INT NULL,
		[T_r_index] INT NULL,
		[F_index] INT NULL,
		[F_r_index] INT NULL,
		[L_of_res] FLOAT NULL,
		[Move_rate] INT NULL,
		[CameoUk06] NVARCHAR(3) NULL,
		[CameoUkg06] NVARCHAR(2) NULL,
		[CameoIncome06] NVARCHAR(2) NULL,
		[CameoIncg06] NVARCHAR(1) NULL,
		[CameoInvestor06] NVARCHAR(2) NULL,
		[CameoInvg06] NVARCHAR(1) NULL,
		[CameoProperty06] NVARCHAR(2) NULL,
		[CameoFinance06] NVARCHAR(2) NULL,
		[CameoFing06] NVARCHAR(1) NULL,
		[CameoUnemploy06] NVARCHAR(2) NULL,
		[AgeScore] FLOAT NULL,
		[AgeBand] INT NULL,
		[TenureScore] FLOAT NULL,
		[TenureBand] INT NULL,
		[CompScore] FLOAT NULL,
		[CompBand] INT NULL,
		[EconScore] FLOAT NULL,
		[EconBand] INT NULL,
		[LifeScore] FLOAT NULL,
		[LifeBand] INT NULL,
		[Millhhld] FLOAT NULL,
		[Dirhhld] FLOAT NULL,
		[SocScore] FLOAT NULL,
		[SocBand] INT NULL,
		[OccScore] FLOAT NULL,
		[OccBand] INT NULL,
		[MortScore] FLOAT NULL,
		[MortBand] INT NULL,
		[HhldShare] FLOAT NULL,
		[AvNumHold] FLOAT NULL,
		[AvNumShares] FLOAT NULL,
		[AvNumComps] FLOAT NULL,
		[AvValShares] FLOAT NULL,
		[UnemMalelt] FLOAT NULL,
		[Unem1824] FLOAT NULL,
		[Unem2539] FLOAT NULL,
		[Unem40pl] FLOAT NULL,
		[UnemScore] FLOAT NULL,
		[UnemBal] INT NULL,
		[UnemRate] FLOAT NULL,
		[UnemDiff] FLOAT NULL,
		[UnemInd] INT NULL,
		[Unemall] FLOAT NULL,
		[UnemallIndex] INT NULL,
		[HousAge] NVARCHAR(5) NULL,
		[HhldDensity] FLOAT NULL,
		[CtaxBand] NVARCHAR(1) NULL,
		[LocationType] INT NULL,
		[NatAvgHouse] INT NULL,
		[HouseScore] FLOAT NULL,
		[HouseBand] INT NULL,
		[PriceDiff] BIGINT NOT NULL,
		[PriceIndex] INT NULL,
		[Activity] INT NULL,
		[RegionalBand] INT NULL,
		[AvgDetVal] INT NULL,
		[AvgDetIndex] INT NULL,
		[AvgSemiVal] INT NULL,
		[AvgSemiIndex] INT NULL,
		[AvgTerrVal] INT NULL,
		[AvgTerrIndex] INT NULL,
		[AvgFlatVal] INT NULL,
		[AvgFlatIndex] INT NULL,
		[RegionCode] INT NULL,
		[CameoIntl] NVARCHAR(2) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CallCreditData PRIMARY KEY ([CallCreditDataID]),
		CONSTRAINT FK_CallCreditData_CallCreditID FOREIGN KEY ([CallCreditID]) REFERENCES [CallCredit] ([CallCreditID]),
	)
END
GO


IF OBJECT_ID('CallCreditDataAccs') IS NULL
BEGIN
	CREATE TABLE [CallCreditDataAccs] (
		[CallCreditDataAccsID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CallCreditDataID] BIGINT NULL,
		[OiaID] INT NULL,
		[AccHolderName] NVARCHAR(164) NULL,
		[Dob] DATETIME NULL,
		[StatusCode] NVARCHAR(10) NULL,
		[StartDate] DATETIME NULL,
		[EndDate] DATETIME NULL,
		[CurrentAddress] BIT NULL,
		[UnDeclaredAddressType] INT NULL,
		[AddressValue] NVARCHAR(440) NULL,
		[DefDate] DATETIME NULL,
		[OrigDefBal] INT NULL,
		[TermBal] INT NULL,
		[DefSatDate] DATETIME NULL,
		[RepoDate] DATETIME NULL,
		[DelinqDate] DATETIME NULL,
		[DelinqBal] INT NULL,
		[AccNo] NVARCHAR(20) NULL,
		[AccSuffix] INT NULL,
		[Joint] INT NULL,
		[Status] NVARCHAR(10) NULL,
		[DateUpdated] DATETIME NULL,
		[AccTypeCode] NVARCHAR(10) NULL,
		[AccGroupId] INT NULL,
		[CurrencyCode] NVARCHAR(10) NULL,
		[Balance] INT NULL,
		[CurCreditLimit] INT NULL,
		[OpenBalance] INT NULL,
		[ArrStartDate] DATETIME NULL,
		[ArrEndDate] DATETIME NULL,
		[PayStartDate] DATETIME NULL,
		[accStartDate] DATETIME NULL,
		[AccEndDate] DATETIME NULL,
		[RegPayment] INT NULL,
		[ExpectedPayment] INT NULL,
		[ActualPayment] INT NULL,
		[RepayPeriod] INT NULL,
		[RepayFreqCode] NVARCHAR(10) NULL,
		[LumpPayment] INT NULL,
		[PenIntAmt] INT NULL,
		[PromotionalRate] BIT NULL,
		[MinimumPayment] BIT NULL,
		[StatementBalance] INT NULL,
		[SupplierName] NVARCHAR(60) NULL,
		[SupplierTypeCode] NVARCHAR(10) NULL,
		[Apacs] BIT NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CallCreditDataAccs PRIMARY KEY ([CallCreditDataAccsID]),
		CONSTRAINT FK_CallCreditDataAccs_CallCreditDataID FOREIGN KEY ([CallCreditDataID]) REFERENCES [CallCreditData] ([CallCreditDataID]),
	)
END
GO


IF OBJECT_ID('CallCreditDataAccsHistory') IS NULL
BEGIN
	CREATE TABLE [CallCreditDataAccsHistory] (
		[CallCreditDataAccsHistoryID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CallCreditDataAccsID] BIGINT NULL,
		[M] DATETIME NULL,
		[Bal] INT NULL,
		[CreditLimit] INT NULL,
		[Acc] NVARCHAR(10) NULL,
		[Pay] NVARCHAR(10) NULL,
		[StmtBal] INT NULL,
		[PayAmt] INT NULL,
		[CashAdvCount] INT NULL,
		[CashAdvTotal] INT NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CallCreditDataAccsHistory PRIMARY KEY ([CallCreditDataAccsHistoryID]),
		CONSTRAINT FK_CallCreditDataAccsHistory_CallCreditDataAccsID FOREIGN KEY ([CallCreditDataAccsID]) REFERENCES [CallCreditDataAccs] ([CallCreditDataAccsID]),
	)
END
GO


IF OBJECT_ID('CallCreditDataAccsNocs') IS NULL
BEGIN
	CREATE TABLE [CallCreditDataAccsNocs] (
		[CallCreditDataAccsNocsID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CallCreditDataAccsID] BIGINT NULL,
		[NoticeType] NVARCHAR(10) NULL,
		[RefNum] NVARCHAR(30) NULL,
		[DateRaised] DATETIME NULL,
		[Text] NVARCHAR(MAX) NULL,
		[NameDetails] NVARCHAR(164) NULL,
		[CurrentAddress] BIT NULL,
		[UnDeclaredAddressType] INT NULL,
		[AddressValue] NVARCHAR(440) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CallCreditDataAccsNocs PRIMARY KEY ([CallCreditDataAccsNocsID]),
		CONSTRAINT FK_CallCreditDataAccsNocs_CallCreditDataAccsID FOREIGN KEY ([CallCreditDataAccsID]) REFERENCES [CallCreditDataAccs] ([CallCreditDataAccsID]),
	)
END
GO


IF OBJECT_ID('CallCreditDataAddressConfs') IS NULL
BEGIN
	CREATE TABLE [CallCreditDataAddressConfs] (
		[CallCreditDataAddressConfsID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CallCreditDataID] BIGINT NULL,
		[PafValid] BIT NULL,
		[OtherResidents] BIT NULL,
		[CurrentAddress] BIT NULL,
		[UnDeclaredAddressType] INT NULL,
		[AddressValue] NVARCHAR(440) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CallCreditDataAddressConfs PRIMARY KEY ([CallCreditDataAddressConfsID]),
		CONSTRAINT FK_CallCreditDataAddressConfs_CallCreditDataID FOREIGN KEY ([CallCreditDataID]) REFERENCES [CallCreditData] ([CallCreditDataID]),
	)
END
GO


IF OBJECT_ID('CallCreditDataAddressConfsResidents') IS NULL
BEGIN
	CREATE TABLE [CallCreditDataAddressConfsResidents] (
		[CallCreditDataAddressConfsResidentsID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CallCreditDataAddressConfsID] BIGINT NULL,
		[MatchType] NVARCHAR(10) NULL,
		[CurrentName] BIT NULL,
		[DeclaredAlias] BIT NULL,
		[NameDetails] NVARCHAR(164) NULL,
		[Duration] NVARCHAR(30) NULL,
		[StartDate] DATETIME NULL,
		[EndDate] DATETIME NULL,
		[ErValid] INT NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CallCreditDataAddressConfsResidents PRIMARY KEY ([CallCreditDataAddressConfsResidentsID]),
		CONSTRAINT FK_CallCreditDataAddressConfsResidents_CallCreditDataAddressConfsID FOREIGN KEY ([CallCreditDataAddressConfsID]) REFERENCES [CallCreditDataAddressConfs] ([CallCreditDataAddressConfsID]),
	)
END
GO


IF OBJECT_ID('CallCreditDataAddressConfsResidentsErHistory') IS NULL
BEGIN
	CREATE TABLE [CallCreditDataAddressConfsResidentsErHistory] (
		[CallCreditDataAddressConfsResidentsErHistoryID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CallCreditDataAddressConfsResidentsID] BIGINT NULL,
		[StartDate] DATETIME NULL,
		[EndDate] DATETIME NULL,
		[Optout] BIT NULL,
		[RollingRoll] BIT NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CallCreditDataAddressConfsResidentsErHistory PRIMARY KEY ([CallCreditDataAddressConfsResidentsErHistoryID]),
		CONSTRAINT FK_CallCreditDataAddressConfsResidentsErHistory_CallCreditDataAddressConfsResidentsID FOREIGN KEY ([CallCreditDataAddressConfsResidentsID]) REFERENCES [CallCreditDataAddressConfsResidents] ([CallCreditDataAddressConfsResidentsId]),
	)
END
GO


IF OBJECT_ID('CallCreditDataAddressConfsResidentsErHistoryNocs') IS NULL
BEGIN
	CREATE TABLE [CallCreditDataAddressConfsResidentsErHistoryNocs] (
		[CallCreditDataAddressConfsResidentsErHistoryNocsID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CallCreditDataAddressConfsResidentsErHistoryID] BIGINT NULL,
		[NoticeType] NVARCHAR(10) NULL,
		[RefNum] NVARCHAR(30) NULL,
		[DateRaised] DATETIME NULL,
		[Text] NVARCHAR(MAX) NULL,
		[NameDetails] NVARCHAR(164) NULL,
		[CurrentAddress] BIT NULL,
		[UnDeclaredAddressType] INT NULL,
		[AddressValue] NVARCHAR(440) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CallCreditDataAddressConfsResidentsErHistoryNocs PRIMARY KEY ([CallCreditDataAddressConfsResidentsErHistoryNocsID]),
		CONSTRAINT FK_CallCreditDataAddressConfsResidentsErHistoryNocs_CallCreditDataAddressConfsResidentsErHistoryID FOREIGN KEY ([CallCreditDataAddressConfsResidentsErHistoryID]) REFERENCES [CallCreditDataAddressConfsResidentsErHistory] ([CallCreditDataAddressConfsResidentsErHistoryID]),
	)
END
GO


IF OBJECT_ID('CallCreditDataAddressConfsResidentsNocs') IS NULL
BEGIN
	CREATE TABLE [CallCreditDataAddressConfsResidentsNocs] (
		[CallCreditDataAddressConfsResidentsNocsID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CallCreditDataAddressConfsResidentsID] BIGINT NULL,
		[NoticeType] NVARCHAR(10) NULL,
		[RefNum] NVARCHAR(30) NULL,
		[DateRaised] DATETIME NULL,
		[Text] NVARCHAR(MAX) NULL,
		[NameDetails] NVARCHAR(164) NULL,
		[CurrentAddress] BIT NULL,
		[UnDeclaredAddressType] INT NULL,
		[AddressValue] NVARCHAR(440) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CallCreditDataAddressConfsResidentsNocs PRIMARY KEY ([CallCreditDataAddressConfsResidentsNocsID]),
		CONSTRAINT FK_CallCreditDataAddressConfsResidentsNocs_CallCreditDataAddressConfsResidentsID FOREIGN KEY ([CallCreditDataAddressConfsResidentsID]) REFERENCES [CallCreditDataAddressConfsResidents] ([CallCreditDataAddressConfsResidentsID]),
	)
END
GO


IF OBJECT_ID('CallCreditDataAddresses') IS NULL
BEGIN
	CREATE TABLE [CallCreditDataAddresses] (
		[CallCreditDataAddressesID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CallCreditDataID] BIGINT NULL,
		[CurrentAddress] BIT NULL,
		[AddressId] INT NULL,
		[Messagecode] INT NULL,
		[UnDeclaredAddressType] INT NULL,
		[AddressValue] NVARCHAR(440) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CallCreditDataAddresses PRIMARY KEY ([CallCreditDataAddressesID]),
		CONSTRAINT FK_CallCreditDataAddresses_CallCreditDataID FOREIGN KEY ([CallCreditDataID]) REFERENCES [CallCreditData] ([CallCreditDataID]),
	)
END
GO


IF OBJECT_ID('CallCreditDataAddressLinks') IS NULL
BEGIN
	CREATE TABLE [CallCreditDataAddressLinks] (
		[CallCreditDataAddressLinksID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CallCreditDataID] BIGINT NULL,
		[CreationDate] DATETIME NULL,
		[LastConfDate] DATETIME NULL,
		[From] INT NULL,
		[To] INT NULL,
		[SupplierName] NVARCHAR(60) NULL,
		[SupplierTypeCode] NVARCHAR(10) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CallCreditDataAddressLinks PRIMARY KEY ([CallCreditDataAddressLinksID]),
		CONSTRAINT FK_CallCreditDataAddressLinks_CallCreditDataID FOREIGN KEY ([CallCreditDataID]) REFERENCES [CallCreditData] ([CallCreditDataID]),
	)
END
GO


IF OBJECT_ID('CallCreditDataAddressLinksNocs') IS NULL
BEGIN
	CREATE TABLE [CallCreditDataAddressLinksNocs] (
		[CallCreditDataAddressLinksNocsID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CallCreditDataAddressLinksID] BIGINT NULL,
		[NoticeType] NVARCHAR(10) NULL,
		[RefNum] NVARCHAR(30) NULL,
		[DateRaised] DATETIME NULL,
		[Text] NVARCHAR(4000) NULL,
		[NameDetails] NVARCHAR(164) NULL,
		[CurrentAddress] BIT NULL,
		[UnDeclaredAddressType] INT NULL,
		[AddressValue] NVARCHAR(440) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CallCreditDataAddressLinksNocs PRIMARY KEY ([CallCreditDataAddressLinksNocsID]),
		CONSTRAINT FK_CallCreditDataAddressLinksNocs_CallCreditDataAddressLinksID FOREIGN KEY ([CallCreditDataAddressLinksID]) REFERENCES [CallCreditDataAddressLinks] ([CallCreditDataAddressLinksID]),
	)
END
GO


IF OBJECT_ID('CallCreditDataAliasLinks') IS NULL
BEGIN
	CREATE TABLE [CallCreditDataAliasLinks] (
		[CallCreditDataAliasLinksID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CallCreditDataID] BIGINT NULL,
		[Declared] BIT NULL,
		[NameBefore] NVARCHAR(164) NULL,
		[Alias] NVARCHAR(164) NULL,
		[CreationDate] DATETIME NULL,
		[LastConfDate] DATETIME NULL,
		[SupplierName] NVARCHAR(60) NULL,
		[SupplierTypeCode] NVARCHAR(10) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CallCreditDataAliasLinks PRIMARY KEY ([CallCreditDataAliasLinksID]),
		CONSTRAINT FK_CallCreditDataAliasLinks_CallCreditDataID FOREIGN KEY ([CallCreditDataID]) REFERENCES [CallCreditData] ([CallCreditDataID]),
	)
END
GO


IF OBJECT_ID('CallCreditDataAliasLinksNocs') IS NULL
BEGIN
	CREATE TABLE [CallCreditDataAliasLinksNocs] (
		[CallCreditDataAliasLinksNocsID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CallCreditDataAliasLinksID] BIGINT NULL,
		[NoticeType] NVARCHAR(10) NULL,
		[RefNum] NVARCHAR(30) NULL,
		[DateRaised] DATETIME NULL,
		[Text] NVARCHAR(4000) NULL,
		[NameDetails] NVARCHAR(164) NULL,
		[CurrentAddress] BIT NULL,
		[UnDeclaredAddressType] INT NULL,
		[AddressValue] NVARCHAR(440) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CallCreditDataAliasLinksNocs PRIMARY KEY ([CallCreditDataAliasLinksNocsID]),
		CONSTRAINT FK_CallCreditDataAliasLinksNocs_CallCreditDataAliasLinksID FOREIGN KEY ([CallCreditDataAliasLinksID]) REFERENCES [CallCreditDataAliasLinks] ([CallCreditDataAliasLinksID]),
	)
END
GO


IF OBJECT_ID('CallCreditDataAssociateLinks') IS NULL
BEGIN
	CREATE TABLE [CallCreditDataAssociateLinks] (
		[CallCreditDataAssociateLinksID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CallCreditDataID] BIGINT NULL,
		[DeclaredAddress] BIT NULL,
		[OiaID] INT NULL,
		[NavLinkID] NVARCHAR(38) NULL,
		[AssociateName] NVARCHAR(164) NULL,
		[CreationDate] DATETIME NULL,
		[LastConfDate] DATETIME NULL,
		[SupplierName] NVARCHAR(60) NULL,
		[SupplierTypeCode] NVARCHAR(10) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CallCreditDataAssociateLinks PRIMARY KEY ([CallCreditDataAssociateLinksID]),
		CONSTRAINT FK_CallCreditDataAssociateLinks_CallCreditDataID FOREIGN KEY ([CallCreditDataID]) REFERENCES [CallCreditData] ([CallCreditDataID]),
	)
END
GO


IF OBJECT_ID('CallCreditDataAssociateLinksNocs') IS NULL
BEGIN
	CREATE TABLE [CallCreditDataAssociateLinksNocs] (
		[CallCreditDataAssociateLinksNocsID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CallCreditDataAssociateLinksID] BIGINT NULL,
		[NoticeType] NVARCHAR(10) NULL,
		[RefNum] NVARCHAR(30) NULL,
		[DateRaised] DATETIME NULL,
		[Text] NVARCHAR(4000) NULL,
		[NameDetails] NVARCHAR(164) NULL,
		[CurrentAddress] BIT NULL,
		[UnDeclaredAddressType] INT NULL,
		[AddressValue] NVARCHAR(440) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CallCreditDataAssociateLinksNocs PRIMARY KEY ([CallCreditDataAssociateLinksNocsID]),
		CONSTRAINT FK_CallCreditDataAssociateLinksNocs_CallCreditDataAssociateLinksID FOREIGN KEY ([CallCreditDataAssociateLinksID]) REFERENCES [CallCreditDataAssociateLinks] ([CallCreditDataAssociateLinksID]),
	)
END
GO


IF OBJECT_ID('CallCreditDataBais') IS NULL
BEGIN
	CREATE TABLE [CallCreditDataBais] (
		[CallCreditDataBaisID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CallCreditDataID] BIGINT NULL,
		[DischargeDate] DATETIME NULL,
		[LineOfBusiness] NVARCHAR(30) NULL,
		[CourtName] NVARCHAR(50) NULL,
		[CurrentStatus] NVARCHAR(10) NULL,
		[Amount] INT NULL,
		[OrderType] NVARCHAR(10) NULL,
		[OrderDate] DATETIME NULL,
		[CaseYear] INT NULL,
		[CaseRef] NVARCHAR(20) NULL,
		[NameDetails] NVARCHAR(164) NULL,
		[TradingName] NVARCHAR(60) NULL,
		[Dob] DATETIME NULL,
		[CurrentAddress] BIT NULL,
		[UnDeclaredAddressType] INT NULL,
		[AddressValue] NVARCHAR(440) NULL,
		[RestrictionType] NVARCHAR(10) NULL,
		[Startdate] DATETIME NULL,
		[Enddate] DATETIME NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CallCreditDataBais PRIMARY KEY ([CallCreditDataBaisID]),
		CONSTRAINT FK_CallCreditDataBais_CallCreditDataID FOREIGN KEY ([CallCreditDataID]) REFERENCES [CallCreditData] ([CallCreditDataID]),
	)
END
GO




IF OBJECT_ID('CallCreditDataBaisNocs') IS NULL
BEGIN
	CREATE TABLE [CallCreditDataBaisNocs] (
		[CallCreditDataBaisNocsID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CallCreditDataBaisID] BIGINT NULL,
		[NoticeType] NVARCHAR(10) NULL,
		[RefNum] NVARCHAR(30) NULL,
		[DateRaised] DATETIME NULL,
		[Text] NVARCHAR(4000) NULL,
		[NameDetails] NVARCHAR(164) NULL,
		[CurrentAddress] BIT NULL,
		[UnDeclaredAddressType] INT NULL,
		[AddressValue] NVARCHAR(440) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CallCreditDataBaisNocs PRIMARY KEY ([CallCreditDataBaisNocsID]),
		CONSTRAINT FK_CallCreditDataBaisNocs_CallCreditDataBaisID FOREIGN KEY ([CallCreditDataBaisID]) REFERENCES [CallCreditDataBais] ([CallCreditDataBaisID]),
	)
END
GO


IF OBJECT_ID('CallCreditDataCifasFiling') IS NULL
BEGIN
	CREATE TABLE [CallCreditDataCifasFiling] (
		[CallCreditDataCifasFilingID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CallCreditDataID] BIGINT NULL,
		[PersonName] NVARCHAR(164) NULL,
		[Dob] DATETIME NULL,
		[CurrentAddressP] BIT NULL,
		[UnDeclaredAddressTypeP] INT NULL,
		[AddressValueP] NVARCHAR(440) NULL,
		[CompanyNumber] NVARCHAR(8) NULL,
		[CompanyName] NVARCHAR(70) NULL,
		[CurrentAddressC] BIT NULL,
		[UnDeclaredAddressTypeC] INT NULL,
		[AddressValueC] NVARCHAR(440) NULL,
		[MemberNumber] INT NULL,
		[CaseReferenceNo] NVARCHAR(6) NULL,
		[MemberName] NVARCHAR(100) NULL,
		[ProductCode] NVARCHAR(10) NULL,
		[FraudCategory] NVARCHAR(10) NULL,
		[ProductDesc] NVARCHAR(150) NULL,
		[FraudDesc] NVARCHAR(50) NULL,
		[InputDate] DATETIME NULL,
		[ExpiryDate] DATETIME NULL,
		[TransactionType] NVARCHAR(10) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CallCreditDataCifasFiling PRIMARY KEY ([CallCreditDataCifasFilingID]),
		CONSTRAINT FK_CallCreditDataCifasFiling_CallCreditDataID FOREIGN KEY ([CallCreditDataID]) REFERENCES [CallCreditData] ([CallCreditDataID]),
	)
END
GO


IF OBJECT_ID('CallCreditDataCifasFilingNocs') IS NULL
BEGIN
	CREATE TABLE [CallCreditDataCifasFilingNocs] (
		[CallCreditDataCifasFilingNocsID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CallCreditDataCifasFilingID] BIGINT NULL,
		[NoticeType] NVARCHAR(10) NULL,
		[Refnum] NVARCHAR(30) NULL,
		[DateRaised] DATETIME NULL,
		[Text] NVARCHAR(MAX) NULL,
		[NameDetails] NVARCHAR(164) NULL,
		[CurrentAddress] BIT NULL,
		[UnDeclaredAddressType] INT NULL,
		[AddressValue] NVARCHAR(440) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CallCreditDataCifasFilingNocs PRIMARY KEY ([CallCreditDataCifasFilingNocsID]),
		CONSTRAINT FK_CallCreditDataCifasFilingNocs_CallCreditDataCifasFilingID FOREIGN KEY ([CallCreditDataCifasFilingID]) REFERENCES [CallCreditDataCifasFiling] ([CallCreditDataCifasFilingID]),
	)
END
GO


IF OBJECT_ID('CallCreditDataCifasPlusCases') IS NULL
BEGIN
	CREATE TABLE [CallCreditDataCifasPlusCases] (
		[CallCreditDataCifasPlusCasesID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CallCreditDataID] BIGINT NULL,
		[CaseId] INT NULL,
		[OwningMember] INT NULL,
		[ManagingMember] INT NULL,
		[CaseType] NVARCHAR(10) NULL,
		[ProductCode] NVARCHAR(10) NULL,
		[Facility] NVARCHAR(10) NULL,
		[SupplyDate] DATETIME NULL,
		[ExpiryDate] DATETIME NULL,
		[ApplicationDate] DATETIME NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CallCreditDataCifasPlusCases PRIMARY KEY ([CallCreditDataCifasPlusCasesID]),
		CONSTRAINT FK_CallCreditDataCifasPlusCases_CallCreditDataID FOREIGN KEY ([CallCreditDataID]) REFERENCES [CallCreditData] ([CallCreditDataID]),
	)
END
GO


IF OBJECT_ID('CallCreditDataCifasPlusCasesDmrs') IS NULL
BEGIN
	CREATE TABLE [CallCreditDataCifasPlusCasesDmrs] (
		[CallCreditDataCifasPlusCasesDmrsID] BIGINT NOT NULL,
		[CallCreditDataCifasPlusCasesID] BIGINT NULL,
		[dmr] INT NOT NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CallCreditDataCifasPlusCasesDmrs PRIMARY KEY ([CallCreditDataCifasPlusCasesDmrsID]),
		CONSTRAINT FK_CallCreditDataCifasPlusCasesDmrs_CallCreditDataCifasPlusCasesID FOREIGN KEY ([CallCreditDataCifasPlusCasesID]) REFERENCES [CallCreditDataCifasPlusCases] ([CallCreditDataCifasPlusCasesID]),
	)
END
GO


IF OBJECT_ID('CallCreditDataCifasPlusCasesFilingReasons') IS NULL
BEGIN
	CREATE TABLE [CallCreditDataCifasPlusCasesFilingReasons] (
		[CallCreditDataCifasPlusCasesFilingReasonsID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CallCreditDataCifasPlusCasesID] BIGINT NULL,
		[FilingReason] NVARCHAR(10) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CallCreditDataCifasPlusCasesFilingReasons PRIMARY KEY ([CallCreditDataCifasPlusCasesFilingReasonsID]),
		CONSTRAINT FK_CallCreditDataCifasPlusCasesFilingReasons_CallCreditDataCifasPlusCasesID FOREIGN KEY ([CallCreditDataCifasPlusCasesID]) REFERENCES [CallCreditDataCifasPlusCases] ([CallCreditDataCifasPlusCasesID]),
	)
END
GO


IF OBJECT_ID('CallCreditDataCifasPlusCasesNocs') IS NULL
BEGIN
	CREATE TABLE [CallCreditDataCifasPlusCasesNocs] (
		[CallCreditDataCifasPlusCasesNocsID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CallCreditDataCifasPlusCasesID] BIGINT NULL,
		[NoticeType] NVARCHAR(10) NULL,
		[Refnum] NVARCHAR(30) NULL,
		[DateRaised] DATETIME NULL,
		[Text] NVARCHAR(4000) NULL,
		[NameDetails] NVARCHAR(164) NULL,
		[CurrentAddress] BIT NULL,
		[UnDeclaredAddressType] INT NULL,
		[AddressValue] NVARCHAR(440) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CallCreditDataCifasPlusCasesNocs PRIMARY KEY ([CallCreditDataCifasPlusCasesNocsID]),
		CONSTRAINT FK_CallCreditDataCifasPlusCasesNocs_CallCreditDataCifasPlusCasesID FOREIGN KEY ([CallCreditDataCifasPlusCasesID]) REFERENCES [CallCreditDataCifasPlusCases] ([CallCreditDataCifasPlusCasesID]),
	)
END
GO


IF OBJECT_ID('CallCreditDataCifasPlusCasesSubjects') IS NULL
BEGIN
	CREATE TABLE [CallCreditDataCifasPlusCasesSubjects] (
		[CallCreditDataCifasPlusCasesSubjectsID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CallCreditDataCifasPlusCasesID] BIGINT NULL,
		[PersonName] NVARCHAR(164) NULL,
		[PersonDob] DATETIME NULL,
		[CompanyName] NVARCHAR(70) NULL,
		[CompanyNumber] NVARCHAR(8) NULL,
		[HomeTelephone] NVARCHAR(20) NULL,
		[MobileTelephone] NVARCHAR(20) NULL,
		[Email] NVARCHAR(60) NULL,
		[SubjectRole] NVARCHAR(10) NULL,
		[SubjectRoleQualifier] NVARCHAR(10) NULL,
		[AddressType] NVARCHAR(10) NULL,
		[CurrentAddress] BIT NULL,
		[UndeclaredAddressType] INT NULL,
		[AddressValue] NVARCHAR(440) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CallCreditDataCifasPlusCasesSubjects PRIMARY KEY ([CallCreditDataCifasPlusCasesSubjectsID]),
		CONSTRAINT FK_CallCreditDataCifasPlusCasesSubjects_CallCreditDataCifasPlusCasesID FOREIGN KEY ([CallCreditDataCifasPlusCasesID]) REFERENCES [CallCreditDataCifasPlusCases] ([CallCreditDataCifasPlusCasesID]),
	)
END
GO


IF OBJECT_ID('CallCreditDataCreditScores') IS NULL
BEGIN
	CREATE TABLE [CallCreditDataCreditScores] (
		[CallCreditDataCreditScoresID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CallCreditDataID] BIGINT NULL,
		[score] INT NULL,
		[ScoreClass] INT NULL,
		[Reason1] INT NULL,
		[Reason2] INT NULL,
		[Reason3] INT NULL,
		[Reason4] INT NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CallCreditDataCreditScores PRIMARY KEY ([CallCreditDataCreditScoresID]),
		CONSTRAINT FK_CallCreditDataCreditScores_CallCreditDataID FOREIGN KEY ([CallCreditDataID]) REFERENCES [CallCreditData] ([CallCreditDataID]),
	)
END
GO


IF OBJECT_ID('CallCreditDataJudgments') IS NULL
BEGIN
	CREATE TABLE [CallCreditDataJudgments] (
		[CallCreditDataJudgmentsID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CallCreditDataID] BIGINT NULL,
		[NameDetails] NVARCHAR(164) NULL,
		[Dob] DATETIME NULL,
		[CourtName] NVARCHAR(50) NULL,
		[CourtType] INT NULL,
		[CaseNumber] NVARCHAR(30) NULL,
		[Status] NVARCHAR(10) NULL,
		[Amount] INT NULL,
		[JudgmentDate] DATETIME NULL,
		[DateSatisfied] DATETIME NULL,
		[CurrentAddress] BIT NULL,
		[UnDeclaredAddressType] INT NULL,
		[AddressValue] NVARCHAR(440) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CallCreditDataJudgments PRIMARY KEY ([CallCreditDataJudgmentsID]),
		CONSTRAINT FK_CallCreditDataJudgments_CallCreditDataID FOREIGN KEY ([CallCreditDataID]) REFERENCES [CallCreditData] ([CallCreditDataID]),
	)
END
GO





IF OBJECT_ID('CallCreditDataJudgmentsNocs') IS NULL
BEGIN
	CREATE TABLE [CallCreditDataJudgmentsNocs] (
		[CallCreditDataJudgmentsNocsID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CallCreditDataJudgmentsID] BIGINT NULL,
		[NoticeType] NVARCHAR(10) NULL,
		[RefNum] NVARCHAR(30) NULL,
		[DateRaised] DATETIME NULL,
		[Text] NVARCHAR(4000) NULL,
		[NameDetails] NVARCHAR(164) NULL,
		[CurrentAddress] BIT NULL,
		[UnDeclaredAddressType] INT NULL,
		[AddressValue] NVARCHAR(440) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CallCreditDataJudgmentsNocs PRIMARY KEY ([CallCreditDataJudgmentsNocsID]),
		CONSTRAINT FK_CallCreditDataJudgmentsNocs_CallCreditDataJudgmentsID FOREIGN KEY ([CallCreditDataJudgmentsID]) REFERENCES [CallCreditDataJudgments] ([CallCreditDataJudgmentsID]),
	)
END
GO


IF OBJECT_ID('CallCreditDataLinkAddresses') IS NULL
BEGIN
	CREATE TABLE [CallCreditDataLinkAddresses] (
		[CallCreditDataLinkAddressesID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CallCreditDataID] BIGINT NULL,
		[AddressID] INT NULL,
		[Declared] BIT NULL,
		[NavLinkID] NVARCHAR(38) NULL,
		[CurrentAddress] BIT NULL,
		[UnDeclaredAddressType] INT NULL,
		[AddressValue] NVARCHAR(440) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CallCreditDataLinkAddresses PRIMARY KEY ([CallCreditDataLinkAddressesID]),
		CONSTRAINT FK_CallCreditDataLinkAddresses_CallCreditDataID FOREIGN KEY ([CallCreditDataID]) REFERENCES [CallCreditData] ([CallCreditDataID]),
	)
END
GO


IF OBJECT_ID('CallCreditDataNocs') IS NULL
BEGIN
	CREATE TABLE [CallCreditDataNocs] (
		[CallCreditDataNocsID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CallCreditDataID] BIGINT NULL,
		[NoticeType] NVARCHAR(10) NULL,
		[Refnum] NVARCHAR(30) NULL,
		[DateRaised] DATETIME NULL,
		[Text] NVARCHAR(4000) NULL,
		[NameDetails] NVARCHAR(164) NULL,
		[CurrentAddress] BIT NULL,
		[UnDeclaredAddressType] INT NULL,
		[AddressValue] NVARCHAR(440) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CallCreditDataNocs PRIMARY KEY ([CallCreditDataNocsID]),
		CONSTRAINT FK_CallCreditDataNocs_CallCreditDataID FOREIGN KEY ([CallCreditDataID]) REFERENCES [CallCreditData] ([CallCreditDataID]),
	)
END
GO


IF OBJECT_ID('CallCreditDataRtr') IS NULL
BEGIN
	CREATE TABLE [CallCreditDataRtr] (
		[CallCreditDataRtrID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CallCreditDataID] BIGINT NULL,
		[HolderName] NVARCHAR(164) NULL,
		[Dob] DATETIME NULL,
		[CurrentAddress] BIT NULL,
		[UnDeclaredAddressType] INT NULL,
		[AddressValue] NVARCHAR(440) NULL,
		[Updated] DATETIME NULL,
		[OrgTypeCode] NVARCHAR(164) NULL,
		[OrgName] NVARCHAR(60) NULL,
		[AccNum] NVARCHAR(20) NULL,
		[AccSuffix] NVARCHAR(10) NULL,
		[AccTypeCode] NVARCHAR(10) NULL,
		[Balance] INT NULL,
		[Limit] INT NULL,
		[StartDate] DATETIME NULL,
		[EndDate] DATETIME NULL,
		[AccStatusCode] NVARCHAR(10) NULL,
		[RepayFreqCode] NVARCHAR(10) NULL,
		[NumOverdue] INT NULL,
		[Rollover] BIT NULL,
		[CrediText] BIT NULL,
		[ChangePay] BIT NULL,
		[NextPayAmount] INT NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CallCreditDataRtr PRIMARY KEY ([CallCreditDataRtrID]),
		CONSTRAINT FK_CallCreditDataRtr_CallCreditDataID FOREIGN KEY ([CallCreditDataID]) REFERENCES [CallCreditData] ([CallCreditDataID]),
	)
END
GO


IF OBJECT_ID('CallCreditDataRtrNocs') IS NULL
BEGIN
	CREATE TABLE [CallCreditDataRtrNocs] (
		[CallCreditDataRtrNocsID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CallCreditDataRtrID] BIGINT NULL,
		[NoticeType] NVARCHAR(10) NULL,
		[Refnum] NVARCHAR(30) NULL,
		[DateRaised] DATETIME NULL,
		[Text] NVARCHAR(4000) NULL,
		[NameDetails] NVARCHAR(164) NULL,
		[CurrentAddress] BIT NULL,
		[UnDeclaredAddressType] INT NULL,
		[AddressValue] NVARCHAR(440) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CallCreditDataRtrNocs PRIMARY KEY ([CallCreditDataRtrNocsID]),
		CONSTRAINT FK_CallCreditDataRtrNocs_CallCreditDataRtrID FOREIGN KEY ([CallCreditDataRtrID]) REFERENCES [CallCreditDataRtr] ([CallCreditDataRtrID]),
	)
END
GO


IF OBJECT_ID('CallCreditDataSearches') IS NULL
BEGIN
	CREATE TABLE [CallCreditDataSearches] (
		[CallCreditDataSearchesID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CallCreditDataID] BIGINT NULL,
		[SearchRef] NVARCHAR(38) NULL,
		[SearchOrgType] NVARCHAR(10) NULL,
		[SearchOrgName] NVARCHAR(50) NULL,
		[YourReference] NVARCHAR(50) NULL,
		[SearchUnitName] NVARCHAR(50) NULL,
		[OwnSearch] BIT NULL,
		[SubsequentEnquiry] BIT NULL,
		[UserName] NVARCHAR(50) NULL,
		[SearchPurpose] NVARCHAR(10) NULL,
		[CreditType] NVARCHAR(10) NULL,
		[Balance] INT NULL,
		[Term] INT NULL,
		[JointApplication] BIT NULL,
		[SearchDate] DATETIME NULL,
		[NameDetailes] NVARCHAR(164) NULL,
		[Dob] DATETIME NULL,
		[StartDate] DATETIME NULL,
		[EndDate] DATETIME NULL,
		[TpOptOut] BIT NULL,
		[Transient] BIT NULL,
		[LinkType] INT NULL,
		[CurrentAddress] BIT NULL,
		[UnDeclaredAddressType] INT NULL,
		[AddressValue] NVARCHAR(440) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CallCreditDataSearches PRIMARY KEY ([CallCreditDataSearchesID]),
		CONSTRAINT FK_CallCreditDataSearches_CallCreditDataID FOREIGN KEY ([CallCreditDataID]) REFERENCES [CallCreditData] ([CallCreditDataID]),
	)
END
GO


IF OBJECT_ID('CallCreditDataTpd') IS NULL
BEGIN
	CREATE TABLE [CallCreditDataTpd] (
		[CallCreditDataTpdID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CallCreditDataID] BIGINT NULL,
		[TotalD] INT NULL,
		[TotalR] INT NULL,
		[Total36mJudgmesntsD] INT NULL,
		[TotalJudgmesntsD] INT NULL,
		[TotalActiveAmountJudgmesntsD] INT NULL,
		[CurrentlyInsolventD] BIT NULL,
		[RestrictedD] BIT NULL,
		[WorsePayStatus12mD] NVARCHAR(2) NULL,
		[WorsePayStatus24mD] NVARCHAR(2) NULL,
		[TotalDefaultsD] INT NULL,
		[TotalDefaults12mD] INT NULL,
		[TotalSettledDefaultsD] INT NULL,
		[TotalDefaultsAmountD] INT NULL,
		[TotalWriteoffsD] INT NULL,
		[TotalWriteoffsAmountD] INT NULL,
		[TotalDelinqsD] INT NULL,
		[TotalDelinqsAmountD] INT NULL,
		[Total36mJudgmesntsR] INT NULL,
		[TotalJudgmesntsR] INT NULL,
		[TotalActiveAmountJudgmesntsR] INT NULL,
		[CurrentlyInsolventR] BIT NULL,
		[RestrictedR] BIT NULL,
		[WorsePayStatus12mR] NVARCHAR(2) NULL,
		[WorsePayStatus24mR] NVARCHAR(2) NULL,
		[TotalDefaultsR] INT NULL,
		[TotalDefaults12mR] INT NULL,
		[TotalSettledDefaultsR] INT NULL,
		[TotalDefaultsAmountR] INT NULL,
		[TotalWriteoffsR] INT NULL,
		[TotalWriteoffsAmountR] INT NULL,
		[TotalDelinqsR] INT NULL,
		[TotalDelinqsAmountR] INT NULL,
		[ThinFile] BIT NULL,
		[TotalH] INT NULL,
		[Total36mJudgmentsH] INT NULL,
		[TotalJudgmentsH] INT NULL,
		[TotalSatisfiedJudgmesntsH] INT NULL,
		[TotalActiveAmountJudgmesntsH] INT NULL,
		[TotalSatisfiedAmountJudgmesntsH] INT NULL,
		[CurrentlyInsolventH] BIT NULL,
		[RestrictedH] BIT NULL,
		[TotalAccountsH] INT NULL,
		[TotalActiveAccountsH] INT NULL,
		[TotalActiveAccountsAmountH] INT NULL,
		[TotalAccountsZerobalH] INT NULL,
		[TotalSettledAccountsAmountH] INT NULL,
		[WorsePayStatus12mH] NVARCHAR(2) NULL,
		[WorsePayStatus24mH] NVARCHAR(2) NULL,
		[TotalDefaultsH] INT NULL,
		[TotalDefaults12mH] INT NULL,
		[TotalDefaultsAmountH] INT NULL,
		[TotalWriteoffsH] INT NULL,
		[TotalWriteoffsAmountH] INT NULL,
		[TotalDelinqsH] INT NULL,
		[TotalDelinqsAmountH] INT NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CallCreditDataTpd PRIMARY KEY ([CallCreditDataTpdID]),
		CONSTRAINT FK_CallCreditDataTpd_CallCreditDataID FOREIGN KEY ([CallCreditDataID]) REFERENCES [CallCreditData] ([CallCreditDataID]),
	)
END
GO


IF OBJECT_ID('CallCreditDataTpdDecisionAlertIndividuals') IS NULL
BEGIN
	CREATE TABLE [CallCreditDataTpdDecisionAlertIndividuals] (
		[CallCreditDataTpdDecisionAlertIndividualsID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CallCreditDataTpdID] BIGINT NULL,
		[IndividualName] NVARCHAR(164) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CallCreditDataTpdDecisionAlertIndividuals PRIMARY KEY ([CallCreditDataTpdDecisionAlertIndividualsID]),
		CONSTRAINT FK_CallCreditDataTpdDecisionAlertIndividuals_CallCreditDataTpdID FOREIGN KEY ([CallCreditDataTpdID]) REFERENCES [CallCreditDataTpd] ([CallCreditDataTpdID]),
	)
END
GO


IF OBJECT_ID('CallCreditDataTpdDecisionAlertIndividualsNocs') IS NULL
BEGIN
	CREATE TABLE [CallCreditDataTpdDecisionAlertIndividualsNocs] (
		[CallCreditDataTpdDecisionAlertIndividualsNocsID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CallCreditDataTpdDecisionAlertIndividualsID] BIGINT NULL,
		[NoticeType] NVARCHAR(10) NULL,
		[Refnum] NVARCHAR(30) NULL,
		[DateRaised] DATETIME NULL,
		[Text] NVARCHAR(4000) NULL,
		[NameDetails] NVARCHAR(164) NULL,
		[CurrentAddress] BIT NULL,
		[UnDeclaredAddressType] INT NULL,
		[AddressValue] NVARCHAR(440) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CallCreditDataTpdDecisionAlertIndividualsNocs PRIMARY KEY ([CallCreditDataTpdDecisionAlertIndividualsNocsID]),
		CONSTRAINT FK_CallCreditDataTpdDecisionAlertIndividualsNocs_CallCreditDataTpdDecisionAlertIndividualsID FOREIGN KEY ([CallCreditDataTpdDecisionAlertIndividualsID]) REFERENCES [CallCreditDataTpdDecisionAlertIndividuals] ([CallCreditDataTpdDecisionAlertIndividualsID]),
	)
END
GO


IF OBJECT_ID('CallCreditDataTpdDecisionCreditScores') IS NULL
BEGIN
	CREATE TABLE [CallCreditDataTpdDecisionCreditScores] (
		[CallCreditDataTpdDecisionCreditScoresID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CallCreditDataTpdID] BIGINT NULL,
		[score] INT NULL,
		[ScoreClass] INT NULL,
		[Reason1] INT NULL,
		[Reason2] INT NULL,
		[Reason3] INT NULL,
		[Reason4] INT NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CallCreditDataTpdDecisionCreditScores PRIMARY KEY ([CallCreditDataTpdDecisionCreditScoresID]),
		CONSTRAINT FK_CallCreditDataTpdDecisionCreditScores_CallCreditDataTpdID FOREIGN KEY ([CallCreditDataTpdID]) REFERENCES [CallCreditDataTpd] ([CallCreditDataTpdID]),
	)
END
GO


IF OBJECT_ID('CallCreditDataTpdHhoCreditScores') IS NULL
BEGIN
	CREATE TABLE [CallCreditDataTpdHhoCreditScores] (
		[CallCreditDataTpdHhoCreditScoresID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CallCreditDataTpdID] BIGINT NULL,
		[score] INT NULL,
		[ScoreClass] INT NULL,
		[Reason1] INT NULL,
		[Reason2] INT NULL,
		[Reason3] INT NULL,
		[Reason4] INT NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CallCreditDataTpdHhoCreditScores PRIMARY KEY ([CallCreditDataTpdHhoCreditScoresID]),
		CONSTRAINT FK_CallCreditDataTpdHhoCreditScores_CallCreditDataTpdID FOREIGN KEY ([CallCreditDataTpdID]) REFERENCES [CallCreditDataTpd] ([CallCreditDataTpdID]),
	)
END
GO


IF OBJECT_ID('CallCreditDataTpdReviewAlertIndividuals') IS NULL
BEGIN
	CREATE TABLE [CallCreditDataTpdReviewAlertIndividuals] (
		[CallCreditDataTpdReviewAlertIndividualsID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CallCreditDataTpdID] BIGINT NULL,
		[IndividualName] NVARCHAR(164) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CallCreditDataTpdReviewAlertIndividuals PRIMARY KEY ([CallCreditDataTpdReviewAlertIndividualsID]),
		CONSTRAINT FK_CallCreditDataTpdReviewAlertIndividuals_CallCreditDataTpdID FOREIGN KEY ([CallCreditDataTpdID]) REFERENCES [CallCreditDataTpd] ([CallCreditDataTpdID]),
	)
END
GO


IF OBJECT_ID('CallCreditDataTpdReviewAlertIndividualsNocs') IS NULL
BEGIN
	CREATE TABLE [CallCreditDataTpdReviewAlertIndividualsNocs] (
		[CallCreditDataTpdReviewAlertIndividualsNocsID] BIGINT IDENTITY(1, 1) NOT NULL,
		[CallCreditDataTpdReviewAlertIndividualsID] BIGINT NULL,
		[NoticeType] NVARCHAR(10) NULL,
		[Refnum] NVARCHAR(30) NULL,
		[DateRaised] DATETIME NULL,
		[Text] NVARCHAR(4000) NULL,
		[NameDetails] NVARCHAR(164) NULL,
		[CurrentAddress] BIT NULL,
		[UnDeclaredAddressType] INT NULL,
		[AddressValue] NVARCHAR(440) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CallCreditDataTpdReviewAlertIndividualsNocs PRIMARY KEY ([CallCreditDataTpdReviewAlertIndividualsNocsID]),
		CONSTRAINT FK_CallCreditDataTpdReviewAlertIndividualsNocs_CallCreditDataTpdReviewAlertIndividualsID FOREIGN KEY ([CallCreditDataTpdReviewAlertIndividualsID]) REFERENCES [CallCreditDataTpdReviewAlertIndividuals] ([CallCreditDataTpdReviewAlertIndividualsID]),
	)
END
GO


