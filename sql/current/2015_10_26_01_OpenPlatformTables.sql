SET QUOTED_IDENTIFIER ON
GO

IF object_id('I_InvestorType') IS NULL
BEGIN
	CREATE TABLE I_InvestorType (
		InvestorTypeID INT NOT NULL IDENTITY(1,1),
		Name NVARCHAR(255) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_I_InvestorType PRIMARY KEY (InvestorTypeID)
	)
END
GO

IF NOT EXISTS (SELECT * FROM I_InvestorType)
BEGIN
	INSERT INTO I_InvestorType (Name) VALUES ('Institutional')
	INSERT INTO I_InvestorType (Name) VALUES ('Private')
	INSERT INTO I_InvestorType (Name) VALUES ('Hedge Fund')
END
GO


IF object_id('I_Investor') IS NULL
BEGIN
	CREATE TABLE I_Investor (
		InvestorID INT NOT NULL IDENTITY(1,1),
		InvestorTypeID INT NOT NULL,
		Name NVARCHAR(255),
		MonthlyFundingCapital DECIMAL(18, 6),
		IsActive BIT NOT NULL,
		Timestamp DATETIME NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_I_Investor PRIMARY KEY (InvestorID),
		CONSTRAINT FK_I_Investor_I_InvestorType FOREIGN KEY (InvestorTypeID) REFERENCES I_InvestorType(InvestorTypeID)
	)
END
GO

IF object_id('I_InvestorContact') IS NULL
BEGIN
	CREATE TABLE I_InvestorContact (
		InvestorContactID INT NOT NULL, -- =Security_User.UserId
		InvestorID INT NOT NULL,
		PersonalName NVARCHAR(255),
		LastName NVARCHAR(255),
		Email NVARCHAR(255),
		Role NVARCHAR(255),
		Comment NVARCHAR(1000),
		IsPrimary BIT NOT NULL,
		Mobile NVARCHAR(30),
		OfficePhone NVARCHAR(30),
		IsActive BIT NOT NULL,
		Timestamp DATETIME NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_I_InvestorContact PRIMARY KEY (InvestorContactID),
		CONSTRAINT FK_I_InvestorContact_I_Investor FOREIGN KEY (InvestorID) REFERENCES I_Investor(InvestorID)
	)
END
GO


IF object_id('I_InvestorAccountType') IS NULL
BEGIN
	CREATE TABLE I_InvestorAccountType (
		InvestorAccountTypeID INT NOT NULL IDENTITY(1,1),
		Name NVARCHAR(255),
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_I_InvestorAccountType PRIMARY KEY (InvestorAccountTypeID)
	)
END
GO

IF NOT EXISTS (SELECT * FROM I_InvestorAccountType)
BEGIN
	INSERT INTO I_InvestorAccountType (Name) VALUES ('Funding')
	INSERT INTO I_InvestorAccountType (Name) VALUES ('Repayments')
	INSERT INTO I_InvestorAccountType (Name) VALUES ('Bridging')
END
GO


IF object_id('I_InvestorBankAccount') IS NULL
BEGIN
	CREATE TABLE I_InvestorBankAccount (
		InvestorBankAccountID INT NOT NULL IDENTITY(1,1),
		InvestorID INT NOT NULL,
		InvestorAccountTypeID INT NOT NULL,
		BankName NVARCHAR(255),
		BankCode NVARCHAR(255),
		BankCountryID NVARCHAR(255),
		BankBranchName NVARCHAR(255),
		BankBranchNumber NVARCHAR(255),
		BankAccountName NVARCHAR(255),
		BankAccountNumber NVARCHAR(255),
		RepaymentKey NVARCHAR(255),
		IsActive BIT NOT NULL,
		Timestamp DATETIME NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_I_InvestorBankAccount PRIMARY KEY (InvestorBankAccountID),
		CONSTRAINT FK_I_InvestorBankAccount_I_Investor FOREIGN KEY (InvestorID) REFERENCES I_Investor(InvestorID),
		CONSTRAINT FK_I_InvestorBankAccount_I_InvestorAccountType FOREIGN KEY (InvestorAccountTypeID) REFERENCES I_InvestorAccountType(InvestorAccountTypeID)
	)
END
GO

IF object_id('I_InvestorBankAccountTransaction') IS NULL
BEGIN
	CREATE TABLE I_InvestorBankAccountTransaction (
		InvestorBankAccountTransactionID INT NOT NULL IDENTITY(1,1),
		InvestorBankAccountID INT NOT NULL,
		PreviousBalance DECIMAL(18,6),
		NewBalance DECIMAL(18,6),
		TransactionAmount DECIMAL(18,6),
		Timestamp DATETIME NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_I_InvestorBankAccountTransaction PRIMARY KEY (InvestorBankAccountTransactionID),
		CONSTRAINT FK_I_InvestorBankAccountTransaction_I_InvestorBankAccount FOREIGN KEY (InvestorBankAccountID) REFERENCES I_InvestorBankAccount(InvestorBankAccountID)
	)
END
GO

IF object_id('I_InvestorSystemBalance') IS NULL
BEGIN
	CREATE TABLE I_InvestorSystemBalance (
		InvestorSystemBalanceID INT NOT NULL IDENTITY(1,1),
		InvestorBankAccountID INT NOT NULL,
		PreviousBalance DECIMAL(18,6),
		NewBalance DECIMAL(18,6),
		TransactionAmount DECIMAL(18,6),
		Timestamp DATETIME NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_I_InvestorSystemBalance PRIMARY KEY (InvestorSystemBalanceID),
		CONSTRAINT FK_I_InvestorSystemBalance_I_InvestorBankAccount FOREIGN KEY (InvestorBankAccountID) REFERENCES I_InvestorBankAccount(InvestorBankAccountID),
	)
END
GO

IF object_id('I_InvestorOverallStatistics') IS NULL
BEGIN
	CREATE TABLE I_InvestorOverallStatistics (
		InvestorOverallStatisticsID INT NOT NULL IDENTITY(1,1),
		InvestorID INT NOT NULL,
		InvestorAccountTypeID INT NOT NULL,		
		TotalYield DECIMAL(18,6),
		TotalAccumulatedRepayments DECIMAL(18,6),
		Defaults DECIMAL(18,6),
		Recoveries DECIMAL(18,6),
		Timestamp DATETIME NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_I_InvestorOverallStatistics PRIMARY KEY (InvestorOverallStatisticsID),
		CONSTRAINT FK_I_InvestorOverallStatistics_I_Investor FOREIGN KEY (InvestorID) REFERENCES I_Investor(InvestorID),
		CONSTRAINT FK_I_InvestorOverallStatistics_I_InvestorAccountType FOREIGN KEY (InvestorAccountTypeID) REFERENCES I_InvestorAccountType(InvestorAccountTypeID)
	)
END
GO

IF object_id('I_Product') IS NULL
BEGIN
	CREATE TABLE I_Product (
		ProductID INT NOT NULL IDENTITY(1,1),
	   	Name NVARCHAR(255),
	   	IsDefault BIT NOT NULL,
	   	IsEnabled BIT NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_I_Product PRIMARY KEY (ProductID)
	)
END
GO

IF NOT EXISTS (SELECT * FROM I_Product)
BEGIN
	INSERT INTO I_Product (Name, IsDefault,IsEnabled) VALUES ('Loans', 1, 1)
	INSERT INTO I_Product (Name, IsDefault,IsEnabled) VALUES ('Alibaba', 0, 1)
	INSERT INTO I_Product (Name, IsDefault,IsEnabled) VALUES ('CreditLine', 0, 1)
	INSERT INTO I_Product (Name, IsDefault,IsEnabled) VALUES ('InvoiceFinance', 0, 1)
END
GO

IF object_id('I_ProductType') IS NULL
BEGIN
	CREATE TABLE I_ProductType (
		ProductTypeID INT NOT NULL IDENTITY(1,1),
	   	ProductID INT NOT NULL,
	   	Name NVARCHAR(255),
		Timestamp DATETIME NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_I_ProductType PRIMARY KEY (ProductTypeID),
		CONSTRAINT FK_I_ProductType_I_Product FOREIGN KEY (ProductID) REFERENCES I_Product(ProductID)
	)
END
GO

IF NOT EXISTS (SELECT * FROM I_ProductType)
BEGIN
	DECLARE @LoansProductID INT = (SELECT ProductID FROM I_Product WHERE Name='Loans')
	INSERT INTO I_ProductType (ProductID, Name, Timestamp) VALUES (@LoansProductID, 'LongTermSMELoans', '2015-12-01')
	INSERT INTO I_ProductType (ProductID, Name, Timestamp) VALUES (@LoansProductID, 'ShortTermSMELoans', '2015-12-01')
END
GO

IF object_id('I_Grade') IS NULL
BEGIN
	CREATE TABLE I_Grade (
		GradeID INT NOT NULL IDENTITY(1,1),
	   	Name NVARCHAR(5),
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_I_Grade PRIMARY KEY (GradeID)
	)
END
GO

IF NOT EXISTS (SELECT * FROM I_Grade)
BEGIN
	INSERT INTO I_Grade (Name) VALUES ('A')
	INSERT INTO I_Grade (Name) VALUES ('B')
	INSERT INTO I_Grade (Name) VALUES ('C')
	INSERT INTO I_Grade (Name) VALUES ('D')
	INSERT INTO I_Grade (Name) VALUES ('E')
	INSERT INTO I_Grade (Name) VALUES ('F')
	INSERT INTO I_Grade (Name) VALUES ('G')
	INSERT INTO I_Grade (Name) VALUES ('H')
END
GO

IF object_id('I_FundingType') IS NULL
BEGIN
	CREATE TABLE I_FundingType (
		FundingTypeID INT NOT NULL IDENTITY(1,1),
	   	Name NVARCHAR(50),
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_I_FundingType PRIMARY KEY (FundingTypeID)
	)
END
GO

IF NOT EXISTS (SELECT * FROM I_FundingType)
BEGIN
	INSERT INTO I_FundingType (Name) VALUES ('CoInvestment')
	INSERT INTO I_FundingType (Name) VALUES ('FullInvestment')
	INSERT INTO I_FundingType (Name) VALUES ('PooledInvestment')
END
GO

IF object_id('I_ProductSubType') IS NULL
BEGIN
	CREATE TABLE I_ProductSubType (
		ProductSubTypeID INT NOT NULL IDENTITY(1,1),
	   	ProductTypeID INT NOT NULL,
	   	GradeID INT NOT NULL,
	   	FundingTypeID INT NULL,
	   	Name NVARCHAR(255),
		Timestamp DATETIME NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_I_ProductSubType PRIMARY KEY (ProductSubTypeID),
		CONSTRAINT FK_I_ProductSubType_I_ProductType FOREIGN KEY (ProductTypeID) REFERENCES I_ProductType(ProductTypeID),
		CONSTRAINT FK_I_ProductSubType_I_Grade FOREIGN KEY (GradeID) REFERENCES I_Grade(GradeID),
		CONSTRAINT FK_I_ProductSubType_I_FundingType FOREIGN KEY (FundingTypeID) REFERENCES I_FundingType(FundingTypeID)
	)
END
GO


IF object_id('I_Portfolio') IS NULL
BEGIN
	CREATE TABLE I_Portfolio (
		PortfolioID INT NOT NULL IDENTITY(1,1),
		InvestorID INT NOT NULL,
		ProductTypeID INT NOT NULL,		
		LoanID INT NOT NULL,
		LoanPercentage DECIMAL(18,6) NOT NULL,
		InitialTerm INT NOT NULL,
		GradeID INT NOT NULL,
		Timestamp DATETIME NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_I_Portfolio PRIMARY KEY (PortfolioID),
		CONSTRAINT FK_I_Portfolio_I_Investor FOREIGN KEY (InvestorID) REFERENCES I_Investor(InvestorID),
		CONSTRAINT FK_I_Portfolio_I_ProductType FOREIGN KEY (ProductTypeID) REFERENCES I_ProductType(ProductTypeID),
		CONSTRAINT FK_I_Portfolio_I_Loan FOREIGN KEY (LoanID) REFERENCES Loan(Id),
		CONSTRAINT FK_I_Portfolio_I_Grade FOREIGN KEY (GradeID) REFERENCES I_Grade(GradeId)
	)
END
GO
 
--IF object_id('I_InvestorFundsAllocation') IS NULL
--BEGIN
--	CREATE TABLE I_InvestorFundsAllocation (
--		InvestorFundsAllocationID INT NOT NULL IDENTITY(1,1),
--		InvestorBankAccountID INT NOT NULL,
--		Amount DECIMAL(18,6) NOT NULL,		
--		AllocationTimestamp DATETIME NOT NULL,
--		ReleaseTimestamp DATETIME NOT NULL,
--		TimestampCounter ROWVERSION,
--		CONSTRAINT PK_I_InvestorFundsAllocation PRIMARY KEY (InvestorFundsAllocationID),
--		CONSTRAINT FK_I_InvestorFundsAllocation_I_InvestorBankAccount FOREIGN KEY (InvestorBankAccountID) REFERENCES I_InvestorBankAccount(InvestorBankAccountID)
--	)
--END
--GO

IF object_id('I_Parameter') IS NULL
BEGIN
	CREATE TABLE I_Parameter (
		ParameterID INT NOT NULL IDENTITY(1,1),
		Name NVARCHAR(255),
		ValueType NVARCHAR(255) NOT NULL,
		DefaultValue DECIMAL(18,6) NULL,		
		MaxLimit DECIMAL(18,6) NULL,		
		MinLimit DECIMAL(18,6) NULL,		
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_I_Parameter PRIMARY KEY (ParameterID)
	)
END
GO

IF object_id('I_InvestorConfigurationParam') IS NULL
BEGIN
	CREATE TABLE I_InvestorConfigurationParam (
		InvestorConfigurationParamID INT NOT NULL IDENTITY(1,1),
		InvestorID INT NOT NULL,
		ParameterID INT NOT NULL,		
		Value DECIMAL(18,6) NOT NULL,		
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_I_InvestorConfigurationParam PRIMARY KEY (InvestorConfigurationParamID),
		CONSTRAINT FK_I_InvestorConfigurationParam_I_Investor FOREIGN KEY (InvestorID) REFERENCES I_Investor(InvestorID),
		CONSTRAINT FK_I_InvestorConfigurationParam_I_Parameter FOREIGN KEY (ParameterID) REFERENCES I_Parameter(ParameterID)
	)
END
GO

IF object_id('I_UWInvestorConfigurationParam') IS NULL
BEGIN
	CREATE TABLE I_UWInvestorConfigurationParam (
		UWInvestorConfigurationParamID INT NOT NULL IDENTITY(1,1),
		InvestorID INT NOT NULL,
		ParameterID INT NOT NULL,		
		Value DECIMAL(18,6) NOT NULL,  
		AllowedForConfig BIT NOT NULL,		
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_I_UWInvestorConfigurationParam PRIMARY KEY (UWInvestorConfigurationParamID),
		CONSTRAINT FK_I_UWInvestorConfigurationParam_I_Investor FOREIGN KEY (InvestorID) REFERENCES I_Investor(InvestorID),
		CONSTRAINT FK_I_UWInvestorConfigurationParam_I_Parameter FOREIGN KEY (ParameterID) REFERENCES I_Parameter(ParameterID)
	)
END
GO

IF object_id('I_Index') IS NULL
BEGIN
	CREATE TABLE I_Index (
		IndexID INT NOT NULL IDENTITY(1,1),
		InvestorID INT NULL,
		ProductTypeID INT NULL,
		IsActive BIT NOT NULL,
		GradeAPercent DECIMAL(18,6) NOT NULL,
		GradeAMinScore DECIMAL(18,6) NOT NULL,
		GradeAMaxScore DECIMAL(18,6) NOT NULL,
		GradeBPercent DECIMAL(18,6) NOT NULL,
		GradeBMinScore DECIMAL(18,6) NOT NULL,
		GradeBMaxScore DECIMAL(18,6) NOT NULL,
		GradeCPercent DECIMAL(18,6) NOT NULL,
		GradeCMinScore DECIMAL(18,6) NOT NULL,
		GradeCMaxScore DECIMAL(18,6) NOT NULL,
		GradeDPercent DECIMAL(18,6) NOT NULL,
		GradeDMinScore DECIMAL(18,6) NOT NULL,
		GradeDMaxScore DECIMAL(18,6) NOT NULL,
		GradeEPercent DECIMAL(18,6) NOT NULL,
		GradeEMinScore DECIMAL(18,6) NOT NULL,
		GradeEMaxScore DECIMAL(18,6) NOT NULL,
		GradeFPercent DECIMAL(18,6) NOT NULL,
		GradeFMinScore DECIMAL(18,6) NOT NULL,
		GradeFMaxScore DECIMAL(18,6) NOT NULL,
		GradeGPercent DECIMAL(18,6) NOT NULL,
		GradeGMinScore DECIMAL(18,6) NOT NULL,
		GradeGMaxScore DECIMAL(18,6) NOT NULL,
		GradeHPercent DECIMAL(18,6) NOT NULL,
		GradeHMinScore DECIMAL(18,6) NOT NULL,
		GradeHMaxScore DECIMAL(18,6) NOT NULL,
		Timestamp DATETIME NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_I_Index PRIMARY KEY (IndexID),
		CONSTRAINT FK_I_Index_I_Investor FOREIGN KEY (InvestorID) REFERENCES I_Investor(InvestorID),
		CONSTRAINT FK_I_Index_I_ProductType FOREIGN KEY (ProductTypeID) REFERENCES I_ProductType(ProductTypeID)
	)
END
GO

IF NOT EXISTS (SELECT * FROM I_Index)
BEGIN
	INSERT INTO I_Index (IsActive, 
	GradeAPercent,GradeAMinScore,GradeAMaxScore, 
	GradeBPercent,GradeBMinScore,GradeBMaxScore, 
	GradeCPercent,GradeCMinScore,GradeCMaxScore, 
	GradeDPercent,GradeDMinScore,GradeDMaxScore, 
	GradeEPercent,GradeEMinScore,GradeEMaxScore, 
	GradeFPercent,GradeFMinScore,GradeFMaxScore, 
	GradeGPercent,GradeGMinScore,GradeGMaxScore, 
	GradeHPercent,GradeHMinScore,GradeHMaxScore, 
	Timestamp) 
	VALUES (1, 
	0.055,  0.002, 0.027,
	0.1749, 0.028, 0.109,
	0.2264, 0.11,  0.253,
	0.1138, 0.254, 0.347,
	0.0637, 0.348, 0.41,
	0.1535, 0.411, 0.552,
	0.1348, 0.553, 0.697,
	0.0779, 0.698, 0.994,
	'2015-12-01')
END
GO

IF object_id('I_Instrument') IS NULL
BEGIN
	CREATE TABLE I_Instrument (
		InstrumentID INT NOT NULL IDENTITY(1,1),
		Name NVARCHAR(50) NOT NULL,
		CurrencyID INT NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_I_Instrument PRIMARY KEY (InstrumentID),
		CONSTRAINT FK_I_Instrument_MP_Currency FOREIGN KEY (CurrencyID) REFERENCES MP_Currency(Id)
	)
END
GO


IF NOT EXISTS (SELECT * FROM I_Instrument)
BEGIN
	DECLARE @GbpID INT = (SELECT Id FROM MP_Currency WHERE Name='GBP' )
	DECLARE @UsdID INT = (SELECT Id FROM MP_Currency WHERE Name='USD' )
	INSERT INTO I_Instrument (Name, CurrencyID) VALUES ('Libor', @GbpID)
	INSERT INTO I_Instrument (Name, CurrencyID) VALUES ('Libor', @UsdID)
END
GO

IF object_id('I_InterestVariable') IS NULL
BEGIN
	CREATE TABLE I_InterestVariable (
		InterestVariableID INT NOT NULL IDENTITY(1,1),
		InstrumentID INT NOT NULL,
		TradeDate DATETIME NOT NULL,
		OneDay BIT NOT NULL,
		OneWeek DECIMAL(18,6) NOT NULL,
		OneMonth DECIMAL(18,6) NOT NULL,
		TwoMonths DECIMAL(18,6) NOT NULL,
		ThreeMonths DECIMAL(18,6) NOT NULL,
		SixMonths DECIMAL(18,6) NOT NULL,
		TwelveMonths DECIMAL(18,6) NOT NULL,
		Timestamp DATETIME NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_I_InterestVariable PRIMARY KEY (InterestVariableID),
		CONSTRAINT FK_I_InterestVariable_I_Instrument FOREIGN KEY (InstrumentID) REFERENCES I_Instrument(InstrumentID)
	)
END
GO



