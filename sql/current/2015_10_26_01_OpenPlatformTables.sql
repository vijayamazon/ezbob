SET QUOTED_IDENTIFIER ON
GO

IF object_id('InvestorType') IS NULL
BEGIN
	CREATE TABLE InvestorType (
		InvestorTypeID INT NOT NULL IDENTITY(1,1),
		Name NVARCHAR(255) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_InvestorType PRIMARY KEY (InvestorTypeID)
	)
END
GO

IF NOT EXISTS (SELECT * FROM InvestorType)
BEGIN
	INSERT INTO InvestorType (Name) VALUES ('Institutional')
	INSERT INTO InvestorType (Name) VALUES ('Private')
	INSERT INTO InvestorType (Name) VALUES ('Hedge Fund')
END
GO


IF object_id('Investor') IS NULL
BEGIN
	CREATE TABLE Investor (
		InvestorID INT NOT NULL IDENTITY(1,1),
		InvestorTypeID INT NOT NULL,
		Name NVARCHAR(255),
		Email NVARCHAR(255),
		Password NVARCHAR(255),
		IsActive BIT NOT NULL,
		Timestamp DATETIME NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_Investor PRIMARY KEY (InvestorID),
		CONSTRAINT FK_Investor_InvestorType FOREIGN KEY (InvestorTypeID) REFERENCES InvestorType(InvestorTypeID)
	)
END
GO

IF object_id('InvestorContact') IS NULL
BEGIN
	CREATE TABLE InvestorContact (
		InvestorContactID INT NOT NULL IDENTITY(1,1),
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
		CONSTRAINT PK_InvestorContact PRIMARY KEY (InvestorContactID),
		CONSTRAINT FK_InvestorContact_Investor FOREIGN KEY (InvestorID) REFERENCES Investor(InvestorID)
	)
END
GO


IF object_id('InvestorAccountType') IS NULL
BEGIN
	CREATE TABLE InvestorAccountType (
		InvestorAccountTypeID INT NOT NULL IDENTITY(1,1),
		Name NVARCHAR(255),
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_InvestorAccountType PRIMARY KEY (InvestorAccountTypeID)
	)
END
GO

IF NOT EXISTS (SELECT * FROM InvestorAccountType)
BEGIN
	INSERT INTO InvestorAccountType (Name) VALUES ('Funding')
	INSERT INTO InvestorAccountType (Name) VALUES ('Repayments')
	INSERT INTO InvestorAccountType (Name) VALUES ('Bridging')
END
GO


IF object_id('InvestorBankAccount') IS NULL
BEGIN
	CREATE TABLE InvestorBankAccount (
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
		CONSTRAINT PK_InvestorBankAccount PRIMARY KEY (InvestorBankAccountID),
		CONSTRAINT FK_InvestorBankAccount_Investor FOREIGN KEY (InvestorID) REFERENCES Investor(InvestorID),
		CONSTRAINT FK_InvestorBankAccount_InvestorAccountType FOREIGN KEY (InvestorAccountTypeID) REFERENCES InvestorAccountType(InvestorAccountTypeID)
	)
END
GO

IF object_id('InvestorBankAccountTransaction') IS NULL
BEGIN
	CREATE TABLE InvestorBankAccountTransaction (
		InvestorBankAccountTransactionID INT NOT NULL IDENTITY(1,1),
		InvestorBankAccountID INT NOT NULL,
		PreviousBalance DECIMAL(18,6),
		NewBalance DECIMAL(18,6),
		TransactionAmount DECIMAL(18,6),
		Timestamp DATETIME NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_InvestorBankAccountTransaction PRIMARY KEY (InvestorBankAccountTransactionID),
		CONSTRAINT FK_InvestorBankAccountTransaction_InvestorBankAccount FOREIGN KEY (InvestorBankAccountID) REFERENCES InvestorBankAccount(InvestorBankAccountID)
	)
END
GO

IF object_id('InvestorSystemBalance') IS NULL
BEGIN
	CREATE TABLE InvestorSystemBalance (
		InvestorSystemBalanceID INT NOT NULL IDENTITY(1,1),
		InvestorBankAccountID INT NOT NULL,
		PreviousBalance DECIMAL(18,6),
		NewBalance DECIMAL(18,6),
		TransactionAmount DECIMAL(18,6),
		Timestamp DATETIME NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_InvestorSystemBalance PRIMARY KEY (InvestorSystemBalanceID),
		CONSTRAINT FK_InvestorSystemBalance_InvestorBankAccount FOREIGN KEY (InvestorBankAccountID) REFERENCES InvestorBankAccount(InvestorBankAccountID),
	)
END
GO

IF object_id('InvestorOverallStatistics') IS NULL
BEGIN
	CREATE TABLE InvestorOverallStatistics (
		InvestorOverallStatisticsID INT NOT NULL IDENTITY(1,1),
		InvestorID INT NOT NULL,
		InvestorAccountTypeID INT NOT NULL,		
		TotalYield DECIMAL(18,6),
		TotalAccumulatedRepayments DECIMAL(18,6),
		Defaults DECIMAL(18,6),
		Recoveries DECIMAL(18,6),
		Timestamp DATETIME NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_InvestorOverallStatistics PRIMARY KEY (InvestorOverallStatisticsID),
		CONSTRAINT FK_InvestorOverallStatistics_Investor FOREIGN KEY (InvestorID) REFERENCES Investor(InvestorID),
		CONSTRAINT FK_InvestorOverallStatistics_InvestorAccountType FOREIGN KEY (InvestorAccountTypeID) REFERENCES InvestorAccountType(InvestorAccountTypeID)
	)
END
GO