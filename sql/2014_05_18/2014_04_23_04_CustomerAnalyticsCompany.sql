SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID('CustomerAnalyticsCompany') IS NULL
BEGIN
	CREATE TABLE CustomerAnalyticsCompany (
		CustomerAnalyticsCompanyID INT IDENTITY(1, 1) NOT NULL,
		CustomerID INT NOT NULL,
		AnalyticsDate DATETIME NOT NULL,
		IsActive BIT NOT NULL,
		Score INT NOT NULL,
		SuggestedAmount DECIMAL(18, 6) NOT NULL,
		AnnualTurnover DECIMAL(18, 6) NOT NULL,
		IncorporationDate DATETIME NULL,
		CONSTRAINT PK_CustomerAnalyticsCompany PRIMARY KEY (CustomerAnalyticsCompanyID),
		CONSTRAINT FK_CustomerAnalyticsCompany_Customer FOREIGN KEY (CustomerID) REFERENCES Customer(Id)
	)
END
GO

IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'IDX_CustomerAnalyticsCompany')
	CREATE NONCLUSTERED INDEX IDX_CustomerAnalyticsCompany ON CustomerAnalyticsCompany(CustomerID) WHERE IsActive = 1
GO
