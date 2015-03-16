IF EXISTS (SELECT * FROM syscolumns WHERE name = 'AnnualTurnover' AND id = OBJECT_ID('CustomerAnalyticsCompany'))
	ALTER TABLE CustomerAnalyticsCompany DROP COLUMN AnnualTurnover
GO

SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID('CustomerAnalyticsLocalData') IS NULL
	CREATE TABLE CustomerAnalyticsLocalData (
		CustomerAnalyticsLocalDataID BIGINT IDENTITY(1, 1) NOT NULL,
		CustomerID INT NOT NULL,
		AnalyticsDate DATETIME NOT NULL,
		IsActive BIT NOT NULL,
		AnnualTurnover DECIMAL(18, 6) NOT NULL,
		TotalSumOfOrdersForLoanOffer DECIMAL(18, 6) NOT NULL,
		MarketplaceSeniorityYears DECIMAL(18, 6) NOT NULL,
		MaxFeedback INT NOT NULL,
		MPsNumber INT NOT NULL,
		FirstRepaymentDatePassed BIT NOT NULL,
		EzbobSeniorityMonths DECIMAL(18, 6) NOT NULL,
		OnTimeLoans INT NOT NULL,
		LatePayments INT NOT NULL,
		EarlyPayments INT NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_CustomerAnalyticsLocalData PRIMARY KEY (CustomerAnalyticsLocalDataID),
		CONSTRAINT FK_CustomerAnalyticsLocalData_Customer FOREIGN KEY (CustomerID) REFERENCES Customer(Id)
	)
GO

IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'IDX_CustomerAnalyticsLocalData')
	CREATE NONCLUSTERED INDEX IDX_CustomerAnalyticsLocalData ON CustomerAnalyticsLocalData(CustomerID) WHERE IsActive = 1
GO
