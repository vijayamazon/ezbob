SET QUOTED_IDENTIFIER ON
GO

IF object_id('I_InvestorType') IS NULL
BEGIN
	CREATE TABLE I_InvestorType (
		InvestorTypeID INT NOT NULL,
		Name NVARCHAR(255) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_I_InvestorType PRIMARY KEY (InvestorTypeID)
	)
END
GO

IF NOT EXISTS (SELECT * FROM I_InvestorType)
BEGIN
	INSERT INTO I_InvestorType (InvestorTypeID, Name) VALUES (1, 'Institutional')
	INSERT INTO I_InvestorType (InvestorTypeID, Name) VALUES (2, 'Private')
	INSERT INTO I_InvestorType (InvestorTypeID, Name) VALUES (3, 'Hedge Fund')
END
GO


IF object_id('I_Investor') IS NULL
BEGIN
	CREATE TABLE I_Investor (
		InvestorID INT NOT NULL IDENTITY(1,1),
		InvestorTypeID INT NOT NULL,
		Name NVARCHAR(255),
		MonthlyFundingCapital DECIMAL(18, 6) NULL,
		FundsTransferDate INT NULL DEFAULT 1,
		DiscountServicingFeePercent DECIMAL(18,6) NULL,
		FundingLimitForNotification DECIMAL(18,6) NULL DEFAULT 250000,
		FundsTransferSchedule NVARCHAR(255),
		RepaymentsTransferSchedule NVARCHAR(255),
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
		IsGettingAlerts BIT NOT NULL  DEFAULT(1),
		IsGettingReports BIT NOT NULL  DEFAULT(0),
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
		InvestorAccountTypeID INT NOT NULL,
		Name NVARCHAR(255),
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_I_InvestorAccountType PRIMARY KEY (InvestorAccountTypeID)
	)
END
GO

IF NOT EXISTS (SELECT * FROM I_InvestorAccountType)
BEGIN
	INSERT INTO I_InvestorAccountType (InvestorAccountTypeID, Name) VALUES (1, 'Funding')
	INSERT INTO I_InvestorAccountType (InvestorAccountTypeID, Name) VALUES (2, 'Repayments')
	INSERT INTO I_InvestorAccountType (InvestorAccountTypeID, Name) VALUES (3, 'Bridging')
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
		UserID INT,
		Timestamp DATETIME NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_I_InvestorBankAccount PRIMARY KEY (InvestorBankAccountID),
		CONSTRAINT FK_I_InvestorBankAccount_I_Investor FOREIGN KEY (InvestorID) REFERENCES I_Investor(InvestorID),
		CONSTRAINT FK_I_InvestorBankAccount_I_InvestorAccountType FOREIGN KEY (InvestorAccountTypeID) REFERENCES I_InvestorAccountType(InvestorAccountTypeID),
		CONSTRAINT FK_I_InvestorBankAccount_Security_User FOREIGN KEY (UserID) REFERENCES Security_User(UserId)
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
		BankTransactionRef NVARCHAR(255),
		Timestamp DATETIME NOT NULL,
		UserID INT,
		Comment NVARCHAR(500),
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_I_InvestorBankAccountTransaction PRIMARY KEY (InvestorBankAccountTransactionID),
		CONSTRAINT FK_I_InvestorBankAccountTransaction_I_InvestorBankAccount FOREIGN KEY (InvestorBankAccountID) REFERENCES I_InvestorBankAccount(InvestorBankAccountID),
		CONSTRAINT FK_I_InvestorBankAccountTransaction_Security_User FOREIGN KEY (UserID) REFERENCES Security_User(UserId)
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
		ServicingFeeAmount DECIMAL(18,6),
		Timestamp DATETIME NOT NULL,
		CashRequestID BIGINT,
		LoanID INT,
		LoanTransactionID INT,
		[Comment] NVARCHAR(500),
		UserID INT NULL,
		TransactionDate DATETIME NULL,
		NLOfferID BIGINT NULL, 
		NLLoanID BIGINT NULL,
		NLPaymentID BIGINT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_I_InvestorSystemBalance PRIMARY KEY (InvestorSystemBalanceID),
		CONSTRAINT FK_I_InvestorSystemBalance_I_InvestorBankAccount FOREIGN KEY (InvestorBankAccountID) REFERENCES I_InvestorBankAccount(InvestorBankAccountID),
		CONSTRAINT FK_I_InvestorSystemBalance_CashRequest FOREIGN KEY (CashRequestID) REFERENCES CashRequests(Id),
		CONSTRAINT FK_I_InvestorSystemBalance_Loan FOREIGN KEY (LoanID) REFERENCES Loan(Id),
		CONSTRAINT FK_I_InvestorSystemBalance_LoanTransaction FOREIGN KEY (LoanTransactionID) REFERENCES LoanTransaction(Id),
		CONSTRAINT FK_I_InvestorSystemBalance_NL_Offers FOREIGN KEY (NLOfferID) REFERENCES NL_Offers(OfferID),
		CONSTRAINT FK_I_InvestorSystemBalance_NL_Loans FOREIGN KEY (NLLoanID) REFERENCES NL_Loans(LoanID),
		CONSTRAINT FK_I_InvestorSystemBalance_NL_Payments FOREIGN KEY (NLPaymentID) REFERENCES NL_Payments(PaymentID)
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
		ProductID INT NOT NULL,
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
	INSERT INTO I_Product (ProductID, Name, IsDefault,IsEnabled) VALUES (1, 'Loans',          1, 1)
	INSERT INTO I_Product (ProductID, Name, IsDefault,IsEnabled) VALUES (2, 'Alibaba',        0, 1)
	INSERT INTO I_Product (ProductID, Name, IsDefault,IsEnabled) VALUES (3, 'CreditLine',     0, 0)
	INSERT INTO I_Product (ProductID, Name, IsDefault,IsEnabled) VALUES (4, 'InvoiceFinance', 0, 0)
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
	DECLARE @AlibabaProductID INT = (SELECT ProductID FROM I_Product WHERE Name='Alibaba')
	INSERT INTO I_ProductType (ProductID, Name, Timestamp) VALUES (@LoansProductID, 'LongTermSMELoans', '2015-12-01')
	INSERT INTO I_ProductType (ProductID, Name, Timestamp) VALUES (@LoansProductID, 'ShortTermSMELoans', '2015-12-01')
	INSERT INTO I_ProductType (ProductID, Name, Timestamp) VALUES (@AlibabaProductID, 'Alibaba', '2015-12-01')
END
GO

IF object_id('I_Grade') IS NULL
BEGIN
	CREATE TABLE I_Grade (
		GradeID INT NOT NULL,
	   	Name NVARCHAR(5),
	   	UpperBound DECIMAL(18, 6) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_I_Grade PRIMARY KEY (GradeID),
		CONSTRAINT UC_I_Grade UNIQUE (Name),
		CONSTRAINT CHK_I_Grade CHECK (LTRIM(RTRIM(Name)) != '')
	)
END
GO

IF NOT EXISTS (SELECT * FROM I_Grade)
BEGIN
	INSERT INTO I_Grade (GradeID,Name,UpperBound) VALUES (1,'A',0.155)
	INSERT INTO I_Grade (GradeID,Name,UpperBound) VALUES (2,'B',0.253)
	INSERT INTO I_Grade (GradeID,Name,UpperBound) VALUES (3,'C',0.347)
	INSERT INTO I_Grade (GradeID,Name,UpperBound) VALUES (4,'D',0.452)
	INSERT INTO I_Grade (GradeID,Name,UpperBound) VALUES (5,'E',0.552)
	INSERT INTO I_Grade (GradeID,Name,UpperBound) VALUES (6,'F',0.594)
	INSERT INTO I_Grade (GradeID,Name,UpperBound) VALUES (7,'G',0.697)
	INSERT INTO I_Grade (GradeID,Name,UpperBound) VALUES (8,'H',1.000)
END
GO

IF object_id('I_SubGrade') IS NULL
BEGIN
	CREATE TABLE I_SubGrade (
		SubGradeID INT NOT NULL IDENTITY(1,1),
		GradeID INT NOT NULL,
	   	Name NVARCHAR(5),
	   	MinScore DECIMAL(18, 6) NULL,
	   	MaxScore DECIMAL(18, 6) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_I_SubGrade PRIMARY KEY (SubGradeID)
	)
END
GO

IF NOT EXISTS (SELECT * FROM I_SubGrade)
BEGIN

	DECLARE @GradeA INT = (SELECT GradeID FROM I_Grade WHERE Name='A')
	DECLARE @GradeB INT = (SELECT GradeID FROM I_Grade WHERE Name='B')
	DECLARE @GradeC INT = (SELECT GradeID FROM I_Grade WHERE Name='C')
	DECLARE @GradeD INT = (SELECT GradeID FROM I_Grade WHERE Name='D')
	DECLARE @GradeE INT = (SELECT GradeID FROM I_Grade WHERE Name='E')
	DECLARE @GradeF INT = (SELECT GradeID FROM I_Grade WHERE Name='F')
	DECLARE @GradeG INT = (SELECT GradeID FROM I_Grade WHERE Name='G')
	DECLARE @GradeH INT = (SELECT GradeID FROM I_Grade WHERE Name='H')
	
	INSERT INTO I_SubGrade (GradeID,Name,MinScore,MaxScore) 
	VALUES 
		(@GradeA,'A1',0.002,0.027),
		(@GradeA,'A2',0.027,0.071),
		(@GradeA,'A3',0.071,0.109),
		(@GradeA,'A4',0.109,0.155),
		(@GradeB,'B1',0.155,0.199),
		(@GradeB,'B2',0.199,0.253),
		(@GradeC,'C1',0.253,0.347),
		(@GradeD,'D1',0.347,0.410),
		(@GradeD,'D2',0.410,0.452),
		(@GradeE,'E1',0.452,0.500),
		(@GradeE,'E2',0.500,0.552),
		(@GradeF,'F1',0.552,0.594),
		(@GradeG,'G1',0.594,0.627),
		(@GradeG,'G2',0.627,0.668),
		(@GradeG,'G3',0.668,0.697),
		(@GradeH,'H1',0.697,0.739),
		(@GradeH,'H2',0.739,0.813),
		(@GradeH,'H3',0.813,0.900),
		(@GradeH,'H4',0.900,0.913),
		(@GradeH,'H5',0.913,0.978),
		(@GradeH,'H6',0.978,0.994)
END
GO


IF object_id('I_FundingType') IS NULL
BEGIN
	CREATE TABLE I_FundingType (
		FundingTypeID INT NOT NULL,
	   	Name NVARCHAR(50),
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_I_FundingType PRIMARY KEY (FundingTypeID)
	)
END
GO

IF NOT EXISTS (SELECT * FROM I_FundingType)
BEGIN
	INSERT INTO I_FundingType (FundingTypeID, Name) VALUES (1, 'CoInvestment')
	INSERT INTO I_FundingType (FundingTypeID, Name) VALUES (2, 'FullInvestment')
	INSERT INTO I_FundingType (FundingTypeID, Name) VALUES (3, 'PooledInvestment')
END
GO

IF object_id('TypeOfBusiness') IS NULL
BEGIN
	CREATE TABLE TypeOfBusiness (
		TypeOfBusinessID INT NOT NULL,
	   	Name NVARCHAR(20),
	   	Description NVARCHAR(255),
	   	IsActive BIT NOT NULL,
	   	IsLimited BIT NOT NULL,
	   	IsRegulated BIT NOT NULL,
		TimestampCounter ROWVERSION
	)
END
GO

IF NOT EXISTS (SELECT * FROM TypeOfBusiness)
BEGIN
	INSERT INTO TypeOfBusiness (TypeOfBusinessID, Name, Description, IsActive, IsLimited, IsRegulated) 
	VALUES (0, 'Entrepreneur','Sole trader (not Inc.)',1,0,1),
		   (1, 'LLP','Limited liability partnership',1,1,0),
		   (2, 'PShip3P','Partnership (less than three)',1,0,1),
		   (3, 'PShip','Partnership (more than three)',1,0,0),
		   (4, 'SoleTrader','Sole trader (Inc.)',0,0,1),
	       (5, 'Limited','Limited company',1,1,0)
END
GO

IF object_id('I_ProductSubType') IS NULL
BEGIN
	CREATE TABLE I_ProductSubType (
		ProductSubTypeID INT NOT NULL IDENTITY(1,1),
	   	ProductTypeID INT NOT NULL,
	   	FundingTypeID INT NULL,
	   	OriginID INT NOT NULL,
	   	LoanSourceID INT NOT NULL,
		IsRegulated BIT NOT NULL,
		Timestamp DATETIME NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_I_ProductSubType PRIMARY KEY (ProductSubTypeID),
		CONSTRAINT FK_I_ProductSubType_I_ProductType FOREIGN KEY (ProductTypeID) REFERENCES I_ProductType(ProductTypeID),
		CONSTRAINT FK_I_ProductSubType_I_FundingType FOREIGN KEY (FundingTypeID) REFERENCES I_FundingType(FundingTypeID),
		CONSTRAINT FK_I_ProductSubType_CustomerOrigin FOREIGN KEY (OriginID) REFERENCES CustomerOrigin(CustomerOriginID),
		CONSTRAINT FK_I_ProductSubType_LoanSource FOREIGN KEY (LoanSourceID) REFERENCES LoanSource(LoanSourceID)
	)
END
GO

IF NOT EXISTS (SELECT * FROM I_ProductSubType)
BEGIN
	DECLARE @ProductTypeLongTerm INT = (SELECT ProductTypeID FROM I_ProductType WHERE Name='LongTermSMELoans')
	DECLARE @ProductTypeShortTerm INT = (SELECT ProductTypeID FROM I_ProductType WHERE Name='ShortTermSMELoans')
	DECLARE @ProductTypeAlibaba INT = (SELECT ProductTypeID FROM I_ProductType WHERE Name='Alibaba')
		
	DECLARE @FundingTypeFull INT = (SELECT FundingTypeID FROM I_FundingType WHERE Name='FullInvestment')
	
	DECLARE @LoanSourceCosme INT = (SELECT LoanSourceID FROM LoanSource WHERE LoanSourceName='COSME')
	DECLARE @LoanSourceStandard INT = (SELECT LoanSourceID FROM LoanSource WHERE LoanSourceName='Standard')
	
	DECLARE @OriginEzbob INT = (SELECT CustomerOriginID FROM CustomerOrigin WHERE Name='ezbob')
	DECLARE @OriginEverline INT = (SELECT CustomerOriginID FROM CustomerOrigin WHERE Name='everline')
	DECLARE @OriginAlibaba INT = (SELECT CustomerOriginID FROM CustomerOrigin WHERE Name='alibaba')
	
	

	INSERT INTO I_ProductSubType (ProductTypeID, FundingTypeID, OriginID, LoanSourceID, Timestamp, IsRegulated)
	VALUES
			-- op everline non regulated
		   (@ProductTypeLongTerm,@FundingTypeFull,@OriginEverline,@LoanSourceCosme,    '2015-12-01', 0),
		   (@ProductTypeLongTerm,@FundingTypeFull,@OriginEverline,@LoanSourceStandard, '2015-12-01', 0),
		   
		   -- ezbob non regulated
		   (@ProductTypeShortTerm,NULL,@OriginEzbob,@LoanSourceCosme,    '2015-12-01', 0),
		   (@ProductTypeShortTerm,NULL,@OriginEzbob,@LoanSourceStandard, '2015-12-01', 0),
		   
		   -- ezbob regulated
		   (@ProductTypeShortTerm,NULL,@OriginEzbob,@LoanSourceCosme,    '2015-12-01', 1),
		   (@ProductTypeShortTerm,NULL,@OriginEzbob,@LoanSourceStandard, '2015-12-01', 1),
		   
		   -- alibaba regulated
		   (@ProductTypeAlibaba,NULL,@OriginAlibaba,@LoanSourceCosme,    '2015-12-01', 0),
		   (@ProductTypeAlibaba,NULL,@OriginAlibaba,@LoanSourceStandard, '2015-12-01', 0),
		   
		   --everline regulated
		   (@ProductTypeShortTerm,NULL,@OriginEverline,@LoanSourceCosme,    '2015-12-01', 1),
		   (@ProductTypeShortTerm,NULL,@OriginEverline,@LoanSourceStandard, '2015-12-01', 1)
END
GO

IF object_id('I_GradeOriginMap') IS NULL
BEGIN
	CREATE TABLE I_GradeOriginMap (
		GradeOriginID INT NOT NULL IDENTITY(1,1),
	   	GradeID INT NOT NULL,
		OriginID INT NOT NULL,
	   	TimestampCounter ROWVERSION,
		CONSTRAINT PK_I_GradeOriginMap PRIMARY KEY (GradeOriginID),
		CONSTRAINT FK_I_GradeOriginMap_CustomerOrigin FOREIGN KEY (OriginID) REFERENCES CustomerOrigin(CustomerOriginID),
		CONSTRAINT FK_I_GradeOriginMap_I_Grade FOREIGN KEY (GradeID) REFERENCES I_Grade(GradeID)
	)
END
GO

IF NOT EXISTS (SELECT * FROM I_GradeOriginMap)
BEGIN
	DECLARE @GradeA INT = (SELECT GradeID FROM I_Grade WHERE Name='A')
	DECLARE @GradeB INT = (SELECT GradeID FROM I_Grade WHERE Name='B')
	DECLARE @GradeC INT = (SELECT GradeID FROM I_Grade WHERE Name='C')
	DECLARE @GradeD INT = (SELECT GradeID FROM I_Grade WHERE Name='D')
	DECLARE @GradeE INT = (SELECT GradeID FROM I_Grade WHERE Name='E')
	DECLARE @GradeF INT = (SELECT GradeID FROM I_Grade WHERE Name='F')
	DECLARE @GradeG INT = (SELECT GradeID FROM I_Grade WHERE Name='G')
	DECLARE @GradeH INT = (SELECT GradeID FROM I_Grade WHERE Name='H')
	
	DECLARE @OriginEzbob INT = (SELECT CustomerOriginID FROM CustomerOrigin WHERE Name='ezbob')
	DECLARE @OriginEverline INT = (SELECT CustomerOriginID FROM CustomerOrigin WHERE Name='everline')
	
	INSERT INTO I_GradeOriginMap (GradeID,OriginID)
	VALUES  (@GradeA,@OriginEverline),
			(@GradeB,@OriginEverline),
			(@GradeC,@OriginEverline),
			(@GradeD,@OriginEverline),
			
			(@GradeA,@OriginEzbob),
			(@GradeB,@OriginEzbob),
			(@GradeC,@OriginEzbob),
			(@GradeD,@OriginEzbob),
			(@GradeE,@OriginEzbob),
			(@GradeF,@OriginEzbob)
		   
END
GO



IF object_id('I_Portfolio') IS NULL
BEGIN
	CREATE TABLE I_Portfolio (
		PortfolioID INT NOT NULL IDENTITY(1,1),
		InvestorID INT NOT NULL,
		ProductTypeID INT,		
		LoanID INT NOT NULL,
		LoanPercentage DECIMAL(18,6) NOT NULL,
		InitialTerm INT NOT NULL,
		GradeID INT NOT NULL,
		[Timestamp] DATETIME NOT NULL,
		NLLoanID BIGINT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_I_Portfolio PRIMARY KEY (PortfolioID),
		CONSTRAINT FK_I_Portfolio_I_Investor FOREIGN KEY (InvestorID) REFERENCES I_Investor(InvestorID),
		CONSTRAINT FK_I_Portfolio_I_ProductType FOREIGN KEY (ProductTypeID) REFERENCES I_ProductType(ProductTypeID),
		CONSTRAINT FK_I_Portfolio_I_Loan FOREIGN KEY (LoanID) REFERENCES Loan(Id),
		CONSTRAINT FK_I_Portfolio_I_Grade FOREIGN KEY (GradeID) REFERENCES I_Grade(GradeId),
		CONSTRAINT FK_I_Portfolio_I_NL_Loans FOREIGN KEY (NLLoanID) REFERENCES NL_Loans(LoanID)
	)
END
GO
 
IF object_id('I_Index') IS NULL
BEGIN
	CREATE TABLE I_Index (
		IndexID INT NOT NULL IDENTITY(1,1),
		InvestorID INT NULL,
		ProductID INT NULL,
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
		CONSTRAINT FK_I_Index_I_Product FOREIGN KEY (ProductID) REFERENCES I_Product(ProductID),
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
	0.3074, 0.000, 0.155,
	0.1489, 0.155, 0.253,
	0.1138, 0.253, 0.347,
	0.0999, 0.347, 0.452,
	0.1173, 0.452, 0.552,
	0.0504, 0.552, 0.594,
	0.0845, 0.594, 0.697,
	0.0779, 0.697, 1.000,
	'2016-02-01')
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

IF object_id('I_GradeRange') IS NULL
BEGIN
	CREATE TABLE I_GradeRange (
		GradeRangeID INT NOT NULL IDENTITY(1,1),
		GradeID INT NULL,
		SubGradeID INT NULL,
		LoanSourceID INT NOT NULL,
		OriginID INT NOT NULL,
		IsFirstLoan BIT NOT NULL,
		MinSetupFee DECIMAL(18,6) NOT NULL,
		MaxSetupFee DECIMAL(18,6) NOT NULL,
		MinInterestRate DECIMAL(18,6) NOT NULL,
		MaxInterestRate DECIMAL(18,6) NOT NULL,
		MinLoanAmount DECIMAL(18,6) NOT NULL,
		MaxLoanAmount DECIMAL(18,6) NOT NULL,
		MinTerm INT NOT NULL,
		MaxTerm INT NOT NULL,
		IsActive BIT NOT NULL,
		Timestamp DATETIME NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_I_GradeRange PRIMARY KEY (GradeRangeID),
		CONSTRAINT FK_I_GradeRange_I_Grade FOREIGN KEY (GradeID) REFERENCES I_Grade(GradeID),
		CONSTRAINT FK_I_GradeRange_I_SubGrade FOREIGN KEY (SubGradeID) REFERENCES I_SubGrade(SubGradeID),
		CONSTRAINT FK_I_GradeRange_LoanSource FOREIGN KEY (LoanSourceID) REFERENCES LoanSource(LoanSourceID),
		CONSTRAINT FK_I_GradeRange_CustomerOrigin FOREIGN KEY (OriginID) REFERENCES CustomerOrigin(CustomerOriginID)
	)
END
GO

IF NOT EXISTS (SELECT * FROM I_GradeRange)
BEGIN

	DECLARE @EzbobOriginID INT = (SELECT CustomerOriginID FROM CustomerOrigin WHERE Name='ezbob' )
	DECLARE @EverlineOriginID INT = (SELECT CustomerOriginID FROM CustomerOrigin WHERE Name='everline' )
	DECLARE @AlibabaOriginID INT = (SELECT CustomerOriginID FROM CustomerOrigin WHERE Name='alibaba' )
	
	DECLARE @StandardSourceID INT = (SELECT LoanSourceID FROM LoanSource WHERE LoanSourceName='Standard')
	DECLARE @CosmeSourceID INT = (SELECT LoanSourceID FROM LoanSource WHERE LoanSourceName='COSME')
	
	DECLARE @GradeA INT = (SELECT GradeID FROM I_Grade WHERE Name='A')
	DECLARE @GradeB INT = (SELECT GradeID FROM I_Grade WHERE Name='B')
	DECLARE @GradeC INT = (SELECT GradeID FROM I_Grade WHERE Name='C')
	DECLARE @GradeD INT = (SELECT GradeID FROM I_Grade WHERE Name='D')
	DECLARE @GradeE INT = (SELECT GradeID FROM I_Grade WHERE Name='E')
	DECLARE @GradeF INT = (SELECT GradeID FROM I_Grade WHERE Name='F')
	DECLARE @GradeG INT = (SELECT GradeID FROM I_Grade WHERE Name='G')
	DECLARE @GradeH INT = (SELECT GradeID FROM I_Grade WHERE Name='H')
	
	DECLARE @SubGradeA1 INT = (SELECT SubGradeID FROM I_SubGrade WHERE Name='A1')
	DECLARE @SubGradeA2 INT = (SELECT SubGradeID FROM I_SubGrade WHERE Name='A2')
	DECLARE @SubGradeA3 INT = (SELECT SubGradeID FROM I_SubGrade WHERE Name='A3')
	DECLARE @SubGradeA4 INT = (SELECT SubGradeID FROM I_SubGrade WHERE Name='A4')
	
	DECLARE @SubGradeB1 INT = (SELECT SubGradeID FROM I_SubGrade WHERE Name='B1')
	DECLARE @SubGradeB2 INT = (SELECT SubGradeID FROM I_SubGrade WHERE Name='B2')
	
	DECLARE @SubGradeC1 INT = (SELECT SubGradeID FROM I_SubGrade WHERE Name='C1')
	
	DECLARE @SubGradeD1 INT = (SELECT SubGradeID FROM I_SubGrade WHERE Name='D1')
	DECLARE @SubGradeD2 INT = (SELECT SubGradeID FROM I_SubGrade WHERE Name='D2')
	
	DECLARE @SubGradeE1 INT = (SELECT SubGradeID FROM I_SubGrade WHERE Name='E1')
	DECLARE @SubGradeE2 INT = (SELECT SubGradeID FROM I_SubGrade WHERE Name='E2')
	
	DECLARE @SubGradeF1 INT = (SELECT SubGradeID FROM I_SubGrade WHERE Name='F1')
	
	DECLARE @SubGradeG1 INT = (SELECT SubGradeID FROM I_SubGrade WHERE Name='G1')
	DECLARE @SubGradeG2 INT = (SELECT SubGradeID FROM I_SubGrade WHERE Name='G2')
	DECLARE @SubGradeG3 INT = (SELECT SubGradeID FROM I_SubGrade WHERE Name='G3')
		
	
	INSERT INTO I_GradeRange 
	       (GradeID, SubGradeID,  LoanSourceID, OriginID, IsFirstLoan, MinSetupFee, MaxSetupFee, MinInterestRate, MaxInterestRate, MinLoanAmount, MaxLoanAmount, MinTerm, MaxTerm, IsActive, Timestamp)
	VALUES
			-- Standard EV/EZ
			-- everline standard new loan
		   (@GradeA, @SubGradeA1, @StandardSourceID, @EverlineOriginID, 1, 0.035,0.035,0.0090,0.0120,1000,120000,3,24,1,'2016-02-01'),
		   (@GradeA, @SubGradeA2, @StandardSourceID, @EverlineOriginID, 1, 0.035,0.035,0.0095,0.0125,1000,120000,3,24,1,'2016-02-01'),
		   (@GradeA, @SubGradeA3, @StandardSourceID, @EverlineOriginID, 1, 0.035,0.035,0.0105,0.0130,1000,120000,3,24,1,'2016-02-01'),
		   (@GradeA, @SubGradeA4, @StandardSourceID, @EverlineOriginID, 1, 0.035,0.035,0.0110,0.0140,1000,120000,3,24,1,'2016-02-01'),
		   (@GradeB, @SubGradeB1, @StandardSourceID, @EverlineOriginID, 1, 0.040,0.040,0.0140,0.0175,1000,120000,3,24,1,'2016-02-01'),
		   (@GradeB, @SubGradeB2, @StandardSourceID, @EverlineOriginID, 1, 0.040,0.040,0.0145,0.0190,1000,120000,3,24,1,'2016-02-01'),
		   (@GradeC, @SubGradeC1, @StandardSourceID, @EverlineOriginID, 1, 0.040,0.040,0.0165,0.0200,1000,100000,3,24,1,'2016-02-01'),
		   (@GradeD, @SubGradeD1, @StandardSourceID, @EverlineOriginID, 1, 0.050,0.050,0.0190,0.0235,1000,90000,3,18,1,'2016-02-01'),
		   (@GradeD, @SubGradeD2, @StandardSourceID, @EverlineOriginID, 1, 0.050,0.050,0.0205,0.0250,1000,90000,3,18,1,'2016-02-01'),
		   (@GradeE, @SubGradeE1, @StandardSourceID, @EverlineOriginID, 1, 0.050,0.050,0.0245,0.0390,1000,50000,3,15,1,'2016-02-01'),
		   (@GradeE, @SubGradeE2, @StandardSourceID, @EverlineOriginID, 1, 0.050,0.050,0.0275,0.0415,1000,50000,3,15,1,'2016-02-01'),
		   (@GradeF, @SubGradeF1, @StandardSourceID, @EverlineOriginID, 1, 0.050,0.050,0.0325,0.0475,1000,35000,3,12,1,'2016-02-01'),
		   (@GradeG, @SubGradeG1, @StandardSourceID, @EverlineOriginID, 1, 0.050,0.050,0.0410,0.0540,1000,20000,3,12,1,'2016-02-01'),
		   (@GradeG, @SubGradeG2, @StandardSourceID, @EverlineOriginID, 1, 0.050,0.050,0.0440,0.0575,1000,20000,3,12,1,'2016-02-01'),
		   (@GradeG, @SubGradeG3, @StandardSourceID, @EverlineOriginID, 1, 0.050,0.050,0.0475,0.0585,1000,20000,3,12,1,'2016-02-01'),
		   --everline standard not new loan
		   (@GradeA, @SubGradeA1, @StandardSourceID, @EverlineOriginID, 0, 0.035,0.035,0.0090,0.0115,1000,120000,3,24,1,'2016-02-01'),
		   (@GradeA, @SubGradeA2, @StandardSourceID, @EverlineOriginID, 0, 0.035,0.035,0.0090,0.0120,1000,120000,3,24,1,'2016-02-01'),
		   (@GradeA, @SubGradeA3, @StandardSourceID, @EverlineOriginID, 0, 0.035,0.035,0.0100,0.0125,1000,120000,3,24,1,'2016-02-01'),
		   (@GradeA, @SubGradeA4, @StandardSourceID, @EverlineOriginID, 0, 0.035,0.035,0.0105,0.0130,1000,120000,3,24,1,'2016-02-01'),
		   (@GradeB, @SubGradeB1, @StandardSourceID, @EverlineOriginID, 0, 0.040,0.040,0.0130,0.0165,1000,120000,3,24,1,'2016-02-01'),
		   (@GradeB, @SubGradeB2, @StandardSourceID, @EverlineOriginID, 0, 0.040,0.040,0.0140,0.0180,1000,120000,3,24,1,'2016-02-01'),
		   (@GradeC, @SubGradeC1, @StandardSourceID, @EverlineOriginID, 0, 0.040,0.040,0.0160,0.0190,1000,100000,3,24,1,'2016-02-01'),
		   (@GradeD, @SubGradeD1, @StandardSourceID, @EverlineOriginID, 0, 0.050,0.050,0.0180,0.0225,1000,90000,3,18,1,'2016-02-01'),
		   (@GradeD, @SubGradeD2, @StandardSourceID, @EverlineOriginID, 0, 0.050,0.050,0.0195,0.0240,1000,90000,3,18,1,'2016-02-01'),
		   (@GradeE, @SubGradeE1, @StandardSourceID, @EverlineOriginID, 0, 0.050,0.050,0.0245,0.0390,1000,50000,3,15,1,'2016-02-01'),
		   (@GradeE, @SubGradeE2, @StandardSourceID, @EverlineOriginID, 0, 0.050,0.050,0.0275,0.0415,1000,50000,3,15,1,'2016-02-01'),
		   (@GradeF, @SubGradeF1, @StandardSourceID, @EverlineOriginID, 0, 0.050,0.050,0.0325,0.0475,1000,35000,3,12,1,'2016-02-01'),
		   (@GradeG, @SubGradeG1, @StandardSourceID, @EverlineOriginID, 0, 0.050,0.050,0.0410,0.0540,1000,20000,3,12,1,'2016-02-01'),
		   (@GradeG, @SubGradeG2, @StandardSourceID, @EverlineOriginID, 0, 0.050,0.050,0.0440,0.0575,1000,20000,3,12,1,'2016-02-01'),
		   (@GradeG, @SubGradeG3, @StandardSourceID, @EverlineOriginID, 0, 0.050,0.050,0.0475,0.0585,1000,20000,3,12,1,'2016-02-01'),
		   	-- ezbob standard new loan
		   (@GradeA, @SubGradeA1, @StandardSourceID, @EzbobOriginID, 1, 0.015,0.050,0.0090,0.0120,1000,120000,3,24,1,'2016-02-01'),
		   (@GradeA, @SubGradeA2, @StandardSourceID, @EzbobOriginID, 1, 0.015,0.050,0.0095,0.0125,1000,120000,3,24,1,'2016-02-01'),
		   (@GradeA, @SubGradeA3, @StandardSourceID, @EzbobOriginID, 1, 0.015,0.050,0.0105,0.0130,1000,120000,3,24,1,'2016-02-01'),
		   (@GradeA, @SubGradeA4, @StandardSourceID, @EzbobOriginID, 1, 0.015,0.050,0.0110,0.0140,1000,120000,3,24,1,'2016-02-01'),
		   (@GradeB, @SubGradeB1, @StandardSourceID, @EzbobOriginID, 1, 0.020,0.055,0.0140,0.0175,1000,120000,3,24,1,'2016-02-01'),
		   (@GradeB, @SubGradeB2, @StandardSourceID, @EzbobOriginID, 1, 0.020,0.055,0.0145,0.0190,1000,120000,3,24,1,'2016-02-01'),
		   (@GradeC, @SubGradeC1, @StandardSourceID, @EzbobOriginID, 1, 0.025,0.060,0.0165,0.0200,1000,100000,3,24,1,'2016-02-01'),
		   (@GradeD, @SubGradeD1, @StandardSourceID, @EzbobOriginID, 1, 0.030,0.065,0.0190,0.0235,1000,90000,3,18,1,'2016-02-01'),
		   (@GradeD, @SubGradeD2, @StandardSourceID, @EzbobOriginID, 1, 0.030,0.065,0.0205,0.0250,1000,90000,3,18,1,'2016-02-01'),
		   (@GradeE, @SubGradeE1, @StandardSourceID, @EzbobOriginID, 1, 0.035,0.070,0.0245,0.0390,1000,50000,3,15,1,'2016-02-01'),
		   (@GradeE, @SubGradeE2, @StandardSourceID, @EzbobOriginID, 1, 0.035,0.070,0.0275,0.0415,1000,50000,3,15,1,'2016-02-01'),
		   (@GradeF, @SubGradeF1, @StandardSourceID, @EzbobOriginID, 1, 0.040,0.075,0.0325,0.0475,1000,35000,3,12,1,'2016-02-01'),
		   (@GradeG, @SubGradeG1, @StandardSourceID, @EzbobOriginID, 1, 0.050,0.080,0.0410,0.0540,1000,20000,3,12,1,'2016-02-01'),
		   (@GradeG, @SubGradeG2, @StandardSourceID, @EzbobOriginID, 1, 0.050,0.080,0.0440,0.0575,1000,20000,3,12,1,'2016-02-01'),
		   (@GradeG, @SubGradeG3, @StandardSourceID, @EzbobOriginID, 1, 0.050,0.080,0.0475,0.0585,1000,20000,3,12,1,'2016-02-01'),
		   --ezbob standard not new loan
		   (@GradeA, @SubGradeA1, @StandardSourceID, @EzbobOriginID, 0, 0.015,0.050,0.0090,0.0115,1000,120000,3,24,1,'2016-02-01'),
		   (@GradeA, @SubGradeA2, @StandardSourceID, @EzbobOriginID, 0, 0.015,0.050,0.0090,0.0120,1000,120000,3,24,1,'2016-02-01'),
		   (@GradeA, @SubGradeA3, @StandardSourceID, @EzbobOriginID, 0, 0.015,0.050,0.0100,0.0125,1000,120000,3,24,1,'2016-02-01'),
		   (@GradeA, @SubGradeA4, @StandardSourceID, @EzbobOriginID, 0, 0.015,0.050,0.0105,0.0130,1000,120000,3,24,1,'2016-02-01'),
		   (@GradeB, @SubGradeB1, @StandardSourceID, @EzbobOriginID, 0, 0.020,0.055,0.0130,0.0165,1000,120000,3,24,1,'2016-02-01'),
		   (@GradeB, @SubGradeB2, @StandardSourceID, @EzbobOriginID, 0, 0.020,0.055,0.0140,0.0180,1000,120000,3,24,1,'2016-02-01'),
		   (@GradeC, @SubGradeC1, @StandardSourceID, @EzbobOriginID, 0, 0.025,0.060,0.0160,0.0190,1000,100000,3,24,1,'2016-02-01'),
		   (@GradeD, @SubGradeD1, @StandardSourceID, @EzbobOriginID, 0, 0.030,0.065,0.0180,0.0225,1000,90000,3,18,1,'2016-02-01'),
		   (@GradeD, @SubGradeD2, @StandardSourceID, @EzbobOriginID, 0, 0.030,0.065,0.0195,0.0240,1000,90000,3,18,1,'2016-02-01'),
		   (@GradeE, @SubGradeE1, @StandardSourceID, @EzbobOriginID, 0, 0.035,0.070,0.0245,0.0390,1000,50000,3,15,1,'2016-02-01'),
		   (@GradeE, @SubGradeE2, @StandardSourceID, @EzbobOriginID, 0, 0.035,0.070,0.0275,0.0415,1000,50000,3,15,1,'2016-02-01'),
		   (@GradeF, @SubGradeF1, @StandardSourceID, @EzbobOriginID, 0, 0.040,0.075,0.0325,0.0475,1000,35000,3,12,1,'2016-02-01'),
		   (@GradeG, @SubGradeG1, @StandardSourceID, @EzbobOriginID, 0, 0.050,0.080,0.0410,0.0540,1000,20000,3,12,1,'2016-02-01'),
		   (@GradeG, @SubGradeG2, @StandardSourceID, @EzbobOriginID, 0, 0.050,0.080,0.0440,0.0575,1000,20000,3,12,1,'2016-02-01'),
		   (@GradeG, @SubGradeG3, @StandardSourceID, @EzbobOriginID, 0, 0.050,0.080,0.0475,0.0585,1000,20000,3,12,1,'2016-02-01'),
		   	
			--COSME EV/EZ
			-- everline cosme new loan
		   (@GradeA, @SubGradeA1, @CosmeSourceID, @EverlineOriginID, 1, 0.035,0.035,0.0090,0.0120,1000,118000,15,24,1,'2016-02-01'),
		   (@GradeA, @SubGradeA2, @CosmeSourceID, @EverlineOriginID, 1, 0.035,0.035,0.0095,0.0125,1000,118000,15,24,1,'2016-02-01'),
		   (@GradeA, @SubGradeA3, @CosmeSourceID, @EverlineOriginID, 1, 0.035,0.035,0.0105,0.0130,1000,118000,15,24,1,'2016-02-01'),
		   (@GradeA, @SubGradeA4, @CosmeSourceID, @EverlineOriginID, 1, 0.035,0.035,0.0110,0.0140,1000,118000,15,24,1,'2016-02-01'),
		   (@GradeB, @SubGradeB1, @CosmeSourceID, @EverlineOriginID, 1, 0.040,0.040,0.0140,0.0175,1000,118000,15,24,1,'2016-02-01'),
		   (@GradeB, @SubGradeB2, @CosmeSourceID, @EverlineOriginID, 1, 0.040,0.040,0.0145,0.0190,1000,118000,15,24,1,'2016-02-01'),
		   (@GradeC, @SubGradeC1, @CosmeSourceID, @EverlineOriginID, 1, 0.040,0.040,0.0165,0.0200,1000,100000,15,24,1,'2016-02-01'),
		   (@GradeD, @SubGradeD1, @CosmeSourceID, @EverlineOriginID, 1, 0.050,0.050,0.0190,0.0235,1000,90000,15,18,1,'2016-02-01'),
		   (@GradeD, @SubGradeD2, @CosmeSourceID, @EverlineOriginID, 1, 0.050,0.050,0.0205,0.0250,1000,90000,15,18,1,'2016-02-01'),
		   (@GradeE, @SubGradeE1, @CosmeSourceID, @EverlineOriginID, 1, 0.050,0.050,0.0245,0.0390,1000,50000,15,15,1,'2016-02-01'),
		   (@GradeE, @SubGradeE2, @CosmeSourceID, @EverlineOriginID, 1, 0.050,0.050,0.0275,0.0415,1000,50000,15,15,1,'2016-02-01'),
		   (@GradeF, @SubGradeF1, @CosmeSourceID, @EverlineOriginID, 1, 0.050,0.050,0.0325,0.0475,1000,35000,15,15,1,'2016-02-01'),
		   (@GradeG, @SubGradeG1, @CosmeSourceID, @EverlineOriginID, 1, 0.050,0.050,0.0410,0.0540,1000,20000,15,15,1,'2016-02-01'),
		   (@GradeG, @SubGradeG2, @CosmeSourceID, @EverlineOriginID, 1, 0.050,0.050,0.0440,0.0575,1000,20000,15,15,1,'2016-02-01'),
		   (@GradeG, @SubGradeG3, @CosmeSourceID, @EverlineOriginID, 1, 0.050,0.050,0.0475,0.0585,1000,20000,15,15,1,'2016-02-01'),
		   --everline cosme not new loan
		   (@GradeA, @SubGradeA1, @CosmeSourceID, @EverlineOriginID, 0, 0.035,0.035,0.0090,0.0115,1000,118000,15,24,1,'2016-02-01'),
		   (@GradeA, @SubGradeA2, @CosmeSourceID, @EverlineOriginID, 0, 0.035,0.035,0.0090,0.0120,1000,118000,15,24,1,'2016-02-01'),
		   (@GradeA, @SubGradeA3, @CosmeSourceID, @EverlineOriginID, 0, 0.035,0.035,0.0100,0.0125,1000,118000,15,24,1,'2016-02-01'),
		   (@GradeA, @SubGradeA4, @CosmeSourceID, @EverlineOriginID, 0, 0.035,0.035,0.0105,0.0130,1000,118000,15,24,1,'2016-02-01'),
		   (@GradeB, @SubGradeB1, @CosmeSourceID, @EverlineOriginID, 0, 0.040,0.040,0.0130,0.0165,1000,118000,15,24,1,'2016-02-01'),
		   (@GradeB, @SubGradeB2, @CosmeSourceID, @EverlineOriginID, 0, 0.040,0.040,0.0140,0.0180,1000,118000,15,24,1,'2016-02-01'),
		   (@GradeC, @SubGradeC1, @CosmeSourceID, @EverlineOriginID, 0, 0.040,0.040,0.0160,0.0190,1000,100000,15,24,1,'2016-02-01'),
		   (@GradeD, @SubGradeD1, @CosmeSourceID, @EverlineOriginID, 0, 0.050,0.050,0.0180,0.0225,1000,90000,15,18,1,'2016-02-01'),
		   (@GradeD, @SubGradeD2, @CosmeSourceID, @EverlineOriginID, 0, 0.050,0.050,0.0195,0.0240,1000,90000,15,18,1,'2016-02-01'),
		   (@GradeE, @SubGradeE1, @CosmeSourceID, @EverlineOriginID, 0, 0.050,0.050,0.0245,0.0390,1000,50000,15,15,1,'2016-02-01'),
		   (@GradeE, @SubGradeE2, @CosmeSourceID, @EverlineOriginID, 0, 0.050,0.050,0.0275,0.0415,1000,50000,15,15,1,'2016-02-01'),
		   (@GradeF, @SubGradeF1, @CosmeSourceID, @EverlineOriginID, 0, 0.050,0.050,0.0325,0.0475,1000,35000,15,15,1,'2016-02-01'),
		   (@GradeG, @SubGradeG1, @CosmeSourceID, @EverlineOriginID, 0, 0.050,0.050,0.0410,0.0540,1000,20000,15,15,1,'2016-02-01'),
		   (@GradeG, @SubGradeG2, @CosmeSourceID, @EverlineOriginID, 0, 0.050,0.050,0.0440,0.0575,1000,20000,15,15,1,'2016-02-01'),
		   (@GradeG, @SubGradeG3, @CosmeSourceID, @EverlineOriginID, 0, 0.050,0.050,0.0475,0.0585,1000,20000,15,15,1,'2016-02-01'),
		   	-- ezbob cosme new loan
		   (@GradeA, @SubGradeA1, @CosmeSourceID, @EzbobOriginID, 1, 0.015,0.050,0.0090,0.0120,1000,118000,15,24,1,'2016-02-01'),
		   (@GradeA, @SubGradeA2, @CosmeSourceID, @EzbobOriginID, 1, 0.015,0.050,0.0095,0.0125,1000,118000,15,24,1,'2016-02-01'),
		   (@GradeA, @SubGradeA3, @CosmeSourceID, @EzbobOriginID, 1, 0.015,0.050,0.0105,0.0130,1000,118000,15,24,1,'2016-02-01'),
		   (@GradeA, @SubGradeA4, @CosmeSourceID, @EzbobOriginID, 1, 0.015,0.050,0.0110,0.0140,1000,118000,15,24,1,'2016-02-01'),
		   (@GradeB, @SubGradeB1, @CosmeSourceID, @EzbobOriginID, 1, 0.020,0.055,0.0140,0.0175,1000,118000,15,24,1,'2016-02-01'),
		   (@GradeB, @SubGradeB2, @CosmeSourceID, @EzbobOriginID, 1, 0.020,0.055,0.0145,0.0190,1000,118000,15,24,1,'2016-02-01'),
		   (@GradeC, @SubGradeC1, @CosmeSourceID, @EzbobOriginID, 1, 0.025,0.060,0.0165,0.0200,1000,100000,15,24,1,'2016-02-01'),
		   (@GradeD, @SubGradeD1, @CosmeSourceID, @EzbobOriginID, 1, 0.030,0.065,0.0190,0.0235,1000,90000,15,18,1,'2016-02-01'),
		   (@GradeD, @SubGradeD2, @CosmeSourceID, @EzbobOriginID, 1, 0.030,0.065,0.0205,0.0250,1000,90000,15,18,1,'2016-02-01'),
		   (@GradeE, @SubGradeE1, @CosmeSourceID, @EzbobOriginID, 1, 0.035,0.070,0.0245,0.0390,1000,50000,15,15,1,'2016-02-01'),
		   (@GradeE, @SubGradeE2, @CosmeSourceID, @EzbobOriginID, 1, 0.035,0.070,0.0275,0.0415,1000,50000,15,15,1,'2016-02-01'),
		   (@GradeF, @SubGradeF1, @CosmeSourceID, @EzbobOriginID, 1, 0.040,0.075,0.0325,0.0475,1000,35000,15,15,1,'2016-02-01'),
		   (@GradeG, @SubGradeG1, @CosmeSourceID, @EzbobOriginID, 1, 0.050,0.080,0.0410,0.0540,1000,20000,15,15,1,'2016-02-01'),
		   (@GradeG, @SubGradeG2, @CosmeSourceID, @EzbobOriginID, 1, 0.050,0.080,0.0440,0.0575,1000,20000,15,15,1,'2016-02-01'),
		   (@GradeG, @SubGradeG3, @CosmeSourceID, @EzbobOriginID, 1, 0.050,0.080,0.0475,0.0585,1000,20000,15,15,1,'2016-02-01'),
		   --ezbob cosme not new loan
		   (@GradeA, @SubGradeA1, @CosmeSourceID, @EzbobOriginID, 0, 0.015,0.050,0.0090,0.0115,1000,118000,15,24,1,'2016-02-01'),
		   (@GradeA, @SubGradeA2, @CosmeSourceID, @EzbobOriginID, 0, 0.015,0.050,0.0090,0.0120,1000,118000,15,24,1,'2016-02-01'),
		   (@GradeA, @SubGradeA3, @CosmeSourceID, @EzbobOriginID, 0, 0.015,0.050,0.0100,0.0125,1000,118000,15,24,1,'2016-02-01'),
		   (@GradeA, @SubGradeA4, @CosmeSourceID, @EzbobOriginID, 0, 0.015,0.050,0.0105,0.0130,1000,118000,15,24,1,'2016-02-01'),
		   (@GradeB, @SubGradeB1, @CosmeSourceID, @EzbobOriginID, 0, 0.020,0.055,0.0130,0.0165,1000,118000,15,24,1,'2016-02-01'),
		   (@GradeB, @SubGradeB2, @CosmeSourceID, @EzbobOriginID, 0, 0.020,0.055,0.0140,0.0180,1000,118000,15,24,1,'2016-02-01'),
		   (@GradeC, @SubGradeC1, @CosmeSourceID, @EzbobOriginID, 0, 0.025,0.060,0.0160,0.0190,1000,100000,15,24,1,'2016-02-01'),
		   (@GradeD, @SubGradeD1, @CosmeSourceID, @EzbobOriginID, 0, 0.030,0.065,0.0180,0.0225,1000,90000,15,18,1,'2016-02-01'),
		   (@GradeD, @SubGradeD2, @CosmeSourceID, @EzbobOriginID, 0, 0.030,0.065,0.0195,0.0240,1000,90000,15,18,1,'2016-02-01'),
		   (@GradeE, @SubGradeE1, @CosmeSourceID, @EzbobOriginID, 0, 0.035,0.070,0.0245,0.0390,1000,50000,15,15,1,'2016-02-01'),
		   (@GradeE, @SubGradeE2, @CosmeSourceID, @EzbobOriginID, 0, 0.035,0.070,0.0275,0.0415,1000,50000,15,15,1,'2016-02-01'),
		   (@GradeF, @SubGradeF1, @CosmeSourceID, @EzbobOriginID, 0, 0.040,0.075,0.0325,0.0475,1000,35000,15,15,1,'2016-02-01'),
		   (@GradeG, @SubGradeG1, @CosmeSourceID, @EzbobOriginID, 0, 0.050,0.080,0.0410,0.0540,1000,20000,15,15,1,'2016-02-01'),
		   (@GradeG, @SubGradeG2, @CosmeSourceID, @EzbobOriginID, 0, 0.050,0.080,0.0440,0.0575,1000,20000,15,15,1,'2016-02-01'),
		   (@GradeG, @SubGradeG3, @CosmeSourceID, @EzbobOriginID, 0, 0.050,0.080,0.0475,0.0585,1000,20000,15,15,1,'2016-02-01'),
		   		   
		    --------------ALIBABA
			
			-- alibaba standard new loan
		   (@GradeA, @SubGradeA1, @StandardSourceID, @AlibabaOriginID, 1, 0.015,0.050,0.0090,0.0120,1000,120000,3,24,1,'2016-02-01'),
		   (@GradeA, @SubGradeA2, @StandardSourceID, @AlibabaOriginID, 1, 0.015,0.050,0.0095,0.0125,1000,120000,3,24,1,'2016-02-01'),
		   (@GradeA, @SubGradeA3, @StandardSourceID, @AlibabaOriginID, 1, 0.015,0.050,0.0105,0.0130,1000,120000,3,24,1,'2016-02-01'),
		   (@GradeA, @SubGradeA4, @StandardSourceID, @AlibabaOriginID, 1, 0.015,0.050,0.0110,0.0140,1000,120000,3,24,1,'2016-02-01'),
		   (@GradeB, @SubGradeB1, @StandardSourceID, @AlibabaOriginID, 1, 0.020,0.055,0.0140,0.0175,1000,120000,3,24,1,'2016-02-01'),
		   (@GradeB, @SubGradeB2, @StandardSourceID, @AlibabaOriginID, 1, 0.020,0.055,0.0145,0.0190,1000,120000,3,24,1,'2016-02-01'),
		   (@GradeC, @SubGradeC1, @StandardSourceID, @AlibabaOriginID, 1, 0.025,0.060,0.0165,0.0200,1000,100000,3,24,1,'2016-02-01'),
		   (@GradeD, @SubGradeD1, @StandardSourceID, @AlibabaOriginID, 1, 0.030,0.065,0.0190,0.0235,1000,90000,3,18,1,'2016-02-01'),
		   (@GradeD, @SubGradeD2, @StandardSourceID, @AlibabaOriginID, 1, 0.030,0.065,0.0205,0.0250,1000,90000,3,18,1,'2016-02-01'),
		   (@GradeE, @SubGradeE1, @StandardSourceID, @AlibabaOriginID, 1, 0.035,0.070,0.0245,0.0390,1000,50000,3,15,1,'2016-02-01'),
		   (@GradeE, @SubGradeE2, @StandardSourceID, @AlibabaOriginID, 1, 0.035,0.070,0.0275,0.0415,1000,50000,3,15,1,'2016-02-01'),
		   (@GradeF, @SubGradeF1, @StandardSourceID, @AlibabaOriginID, 1, 0.040,0.075,0.0325,0.0475,1000,35000,3,12,1,'2016-02-01'),
		   (@GradeG, @SubGradeG1, @StandardSourceID, @AlibabaOriginID, 1, 0.050,0.080,0.0410,0.0540,1000,20000,3,12,1,'2016-02-01'),
		   (@GradeG, @SubGradeG2, @StandardSourceID, @AlibabaOriginID, 1, 0.050,0.080,0.0440,0.0575,1000,20000,3,12,1,'2016-02-01'),
		   (@GradeG, @SubGradeG3, @StandardSourceID, @AlibabaOriginID, 1, 0.050,0.080,0.0475,0.0585,1000,20000,3,12,1,'2016-02-01'),
		   --alibaba standard not new loan
		   (@GradeA, @SubGradeA1, @StandardSourceID, @AlibabaOriginID, 0, 0.015,0.050,0.0090,0.0115,1000,120000,3,24,1,'2016-02-01'),
		   (@GradeA, @SubGradeA2, @StandardSourceID, @AlibabaOriginID, 0, 0.015,0.050,0.0090,0.0120,1000,120000,3,24,1,'2016-02-01'),
		   (@GradeA, @SubGradeA3, @StandardSourceID, @AlibabaOriginID, 0, 0.015,0.050,0.0100,0.0125,1000,120000,3,24,1,'2016-02-01'),
		   (@GradeA, @SubGradeA4, @StandardSourceID, @AlibabaOriginID, 0, 0.015,0.050,0.0105,0.0130,1000,120000,3,24,1,'2016-02-01'),
		   (@GradeB, @SubGradeB1, @StandardSourceID, @AlibabaOriginID, 0, 0.020,0.055,0.0130,0.0165,1000,120000,3,24,1,'2016-02-01'),
		   (@GradeB, @SubGradeB2, @StandardSourceID, @AlibabaOriginID, 0, 0.020,0.055,0.0140,0.0180,1000,120000,3,24,1,'2016-02-01'),
		   (@GradeC, @SubGradeC1, @StandardSourceID, @AlibabaOriginID, 0, 0.025,0.060,0.0160,0.0190,1000,100000,3,24,1,'2016-02-01'),
		   (@GradeD, @SubGradeD1, @StandardSourceID, @AlibabaOriginID, 0, 0.030,0.065,0.0180,0.0225,1000,90000,3,18,1,'2016-02-01'),
		   (@GradeD, @SubGradeD2, @StandardSourceID, @AlibabaOriginID, 0, 0.030,0.065,0.0195,0.0240,1000,90000,3,18,1,'2016-02-01'),
		   (@GradeE, @SubGradeE1, @StandardSourceID, @AlibabaOriginID, 0, 0.035,0.070,0.0245,0.0390,1000,50000,3,15,1,'2016-02-01'),
		   (@GradeE, @SubGradeE2, @StandardSourceID, @AlibabaOriginID, 0, 0.035,0.070,0.0275,0.0415,1000,50000,3,15,1,'2016-02-01'),
		   (@GradeF, @SubGradeF1, @StandardSourceID, @AlibabaOriginID, 0, 0.040,0.075,0.0325,0.0475,1000,35000,3,12,1,'2016-02-01'),
		   (@GradeG, @SubGradeG1, @StandardSourceID, @AlibabaOriginID, 0, 0.050,0.080,0.0410,0.0540,1000,20000,3,12,1,'2016-02-01'),
		   (@GradeG, @SubGradeG2, @StandardSourceID, @AlibabaOriginID, 0, 0.050,0.080,0.0440,0.0575,1000,20000,3,12,1,'2016-02-01'),
		   (@GradeG, @SubGradeG3, @StandardSourceID, @AlibabaOriginID, 0, 0.050,0.080,0.0475,0.0585,1000,20000,3,12,1,'2016-02-01'),			
			
		   --no grade standard
		   (NULL, NULL, @StandardSourceID, @AlibabaOriginID,  0, 0.015,0.080,0.0090,0.0585,1000,120000,3,24,1,'2016-02-01'),
		   (NULL, NULL, @StandardSourceID, @AlibabaOriginID,  1, 0.015,0.080,0.0090,0.0585,1000,120000,3,24,1,'2016-02-01'),
		   
		   (NULL, NULL, @StandardSourceID, @EzbobOriginID,    0, 0.015,0.080,0.0090,0.0585,1000,120000,3,24,1,'2016-02-01'),
		   (NULL, NULL, @StandardSourceID, @EzbobOriginID,    1, 0.015,0.080,0.0090,0.0585,1000,120000,3,24,1,'2016-02-01'),
		   
		   (NULL, NULL, @StandardSourceID, @EverlineOriginID, 0, 0.015,0.080,0.0090,0.0585,1000,120000,3,24,1,'2016-02-01'),
		   (NULL, NULL, @StandardSourceID, @EverlineOriginID, 1, 0.015,0.080,0.0090,0.0585,1000,120000,3,24,1,'2016-02-01'),
		   --no grade cosme
		   (NULL, NULL, @CosmeSourceID, @AlibabaOriginID,  0, 0.015,0.080,0.0090,0.0585,1000,118000,15,24,1,'2016-02-01'),
		   (NULL, NULL, @CosmeSourceID, @AlibabaOriginID,  1, 0.015,0.080,0.0090,0.0585,1000,118000,15,24,1,'2016-02-01'),
		   
		   (NULL, NULL, @CosmeSourceID, @EzbobOriginID,    0, 0.015,0.080,0.0090,0.0585,1000,118000,15,24,1,'2016-02-01'),
		   (NULL, NULL, @CosmeSourceID, @EzbobOriginID,    1, 0.015,0.080,0.0090,0.0585,1000,118000,15,24,1,'2016-02-01'),
		   
		   (NULL, NULL, @CosmeSourceID, @EverlineOriginID, 0, 0.015,0.080,0.0090,0.0585,1000,118000,15,24,1,'2016-02-01'),
		   (NULL, NULL, @CosmeSourceID, @EverlineOriginID, 1, 0.015,0.080,0.0090,0.0585,1000,118000,15,24,1,'2016-02-01')
		   
		   
		   
		   
		   
		 
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

IF object_id('I_OpenPlatformOffer') IS NULL
BEGIN
	CREATE TABLE I_OpenPlatformOffer (
		OpenPlatformOfferID INT NOT NULL IDENTITY(1,1),
		CashRequestID BIGINT NOT NULL,
		InvestorID INT NOT NULL,
		InvestmentPercent DECIMAL(18,6) NOT NULL,
		NLOfferID BIGINT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_I_OpenPlatformOffer PRIMARY KEY (OpenPlatformOfferID),
		CONSTRAINT FK_I_OpenPlatformOffer_CashRequest FOREIGN KEY (CashRequestID) REFERENCES CashRequests(Id),
		CONSTRAINT FK_I_OpenPlatformOffer_I_Investor FOREIGN KEY (InvestorID) REFERENCES I_Investor(InvestorID),
		CONSTRAINT FK_I_OpenPlatformOffer_NL_Offers FOREIGN KEY (NLOfferID) REFERENCES NL_Offers(OfferID)
	)
END
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id=object_id('CashRequests') AND name='ProductSubTypeID')
BEGIN
	ALTER TABLE CashRequests ADD ProductSubTypeID INT 
	ALTER TABLE CashRequests ADD CONSTRAINT FK_CashRequests_I_ProductSubType FOREIGN KEY (ProductSubTypeID) REFERENCES I_ProductSubType(ProductSubTypeID)
END
GO

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

IF object_id('I_InvestorParams') IS NULL
BEGIN
	CREATE TABLE I_InvestorParams (
		InvestorParamsID INT NOT NULL IDENTITY(1,1),
		InvestorID INT NULL,
		ParameterID INT NOT NULL,		
		Value NVARCHAR(255) NOT NULL,	
		Type INT NOT NULL,	
		AllowedForConfig BIT NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_I_I_InvestorParams PRIMARY KEY (InvestorParamsID),
		CONSTRAINT FK_I_I_InvestorParams_I_Investor FOREIGN KEY (InvestorID) REFERENCES I_Investor(InvestorID),
		CONSTRAINT FK_I_I_InvestorParams_I_Parameter FOREIGN KEY (ParameterID) REFERENCES I_Parameter(ParameterID)
	)
END
GO


IF object_id('I_RuleType') IS NULL
BEGIN
	CREATE TABLE I_RuleType (
		RuleTypeID INT NOT NULL,
		Name NVARCHAR(255) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_I_RuleTypeID PRIMARY KEY (RuleTypeID)
	)
END
GO

IF NOT EXISTS (SELECT * FROM I_RuleType)
BEGIN
	INSERT INTO I_RuleType (RuleTypeID,Name) VALUES (1,'System')
	INSERT INTO I_RuleType (RuleTypeID,Name) VALUES (2,'UnderWriter')
	INSERT INTO I_RuleType (RuleTypeID,Name) VALUES (3,'Investor')
END
GO

IF object_id('I_Operator') IS NULL
BEGIN
	CREATE TABLE I_Operator (
		OperatorID INT NOT NULL,
		Name NVARCHAR(255) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_I_OperatorID PRIMARY KEY (OperatorID)
	)
END
GO

IF NOT EXISTS (SELECT * FROM I_Operator)
BEGIN
	INSERT INTO I_Operator (OperatorID,Name) VALUES (1,'Or')
	INSERT INTO I_Operator (OperatorID,Name) VALUES (2,'And')
	INSERT INTO I_Operator (OperatorID,Name) VALUES (3,'GreaterThan')
	INSERT INTO I_Operator (OperatorID,Name) VALUES (4,'LessThan')
	INSERT INTO I_Operator (OperatorID,Name) VALUES (5,'Equal')
	INSERT INTO I_Operator (OperatorID,Name) VALUES (6,'NotEqual')
	INSERT INTO I_Operator (OperatorID,Name) VALUES (7,'Not')
	INSERT INTO I_Operator (OperatorID,Name) VALUES (8,'IsTrue')
END
GO

IF object_id('I_InvestorRule') IS NULL
BEGIN
CREATE TABLE [dbo].[I_InvestorRule](
	[RuleID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NULL,
	[RuleType] [int] NULL,
	[InvestorID] [int] NULL,
	[FuncName] [nvarchar](256) NULL,
	[MemberNameSource] [nvarchar](256) NULL,
	[MemberNameTarget] [nvarchar](256) NULL,
	[LeftParamID] [int] NULL,
	[RightParamID] [int] NULL,
	[Operator] [int] NOT NULL,
	[IsRoot] [bit] NOT NULL,
	CONSTRAINT FK_I_InvestorRuleI_RuleType FOREIGN KEY (RuleType) REFERENCES I_RuleType(RuleTypeID),
	CONSTRAINT FK_I_InvestorRuleI_Operator FOREIGN KEY (Operator) REFERENCES I_Operator(OperatorID),
	CONSTRAINT FK_I_InvestorRule_I_Investor FOREIGN KEY (InvestorID) REFERENCES I_Investor(InvestorID)
	)
END
GO

ALTER TABLE I_InvestorParams ALTER COLUMN Value  varchar(256);

IF NOT EXISTS (SELECT * FROM I_Parameter)
BEGIN
	INSERT INTO I_Parameter(Name,ValueType,DefaultValue,MaxLimit,MinLimit) VALUES ('DailyInvestmentAllowed', 'Decimal', 0, NULL, NULL)
	INSERT INTO I_Parameter(Name,ValueType,DefaultValue,MaxLimit,MinLimit) VALUES('WeeklyInvestmentAllowed', 'Decimal', 0, NULL, NULL)
END
GO

IF NOT EXISTS (SELECT * FROM I_InvestorParams)
BEGIN
	INSERT INTO I_InvestorParams(InvestorID,ParameterID,Value,Type,AllowedForConfig) VALUES(NULL, 1, 1000000, 1,1)
	INSERT INTO I_InvestorParams(InvestorID,ParameterID,Value,Type,AllowedForConfig) VALUES(NULL, 2, 3000000, 1,1)
END
GO

IF NOT EXISTS (SELECT * FROM I_InvestorRule)
BEGIN
	INSERT INTO I_InvestorRule(UserID,RuleType,InvestorID,FuncName,MemberNameSource,MemberNameTarget,LeftParamID,RightParamID,Operator,IsRoot) VALUES(1,1,null,null,null,null,2,3,1,1)
	INSERT INTO I_InvestorRule(UserID,RuleType,InvestorID,FuncName,MemberNameSource,MemberNameTarget,LeftParamID,RightParamID,Operator,IsRoot)VALUES (1,1,null,null,null,null,4,5,1,0)
	INSERT INTO I_InvestorRule(UserID,RuleType,InvestorID,FuncName,MemberNameSource,MemberNameTarget,LeftParamID,RightParamID,Operator,IsRoot) VALUES (1,1,null,null,null,null,6,7,1,0)
	INSERT INTO I_InvestorRule(UserID,RuleType,InvestorID,FuncName,MemberNameSource,MemberNameTarget,LeftParamID,RightParamID,Operator,IsRoot) VALUES (1,1,null,null,'ManagerApprovedSum','DailyAvailableAmount',null,null,3,0)
	INSERT INTO I_InvestorRule(UserID,RuleType,InvestorID,FuncName,MemberNameSource,MemberNameTarget,LeftParamID,RightParamID,Operator,IsRoot) VALUES (1,1,null,null,'ManagerApprovedSum','WeeklyAvailableAmount',null,null,3,0)
	INSERT INTO I_InvestorRule(UserID,RuleType,InvestorID,FuncName,MemberNameSource,MemberNameTarget,LeftParamID,RightParamID,Operator,IsRoot) VALUES (1,1,null,null,'ManagerApprovedSum','Balance',null,null,3,0)
	INSERT INTO I_InvestorRule(UserID,RuleType,InvestorID,FuncName,MemberNameSource,MemberNameTarget,LeftParamID,RightParamID,Operator,IsRoot) VALUES (1,1,null,'RuleBadgetLevel',null,null,null,null,7,0)
END
GO
