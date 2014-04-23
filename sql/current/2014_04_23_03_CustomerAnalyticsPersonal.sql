IF OBJECT_ID('CustomerAnalyticsPersonal') IS NULL
BEGIN
	CREATE TABLE CustomerAnalyticsPersonal (
		CustomerAnalyticsPersonalID INT IDENTITY(1, 1) NOT NULL,
		CustomerID INT NOT NULL,
		AnalyticsDate DATETIME NOT NULL,
		IsActive BIT NOT NULL,
		Score INT NOT NULL,
		MinScore INT NOT NULL,
		MaxScore INT NOT NULL,
		IndebtednessIndex INT NOT NULL,
		NumOfAccounts INT NOT NULL,
		NumOfDefaults INT NOT NULL,
		NumOfLastDefaults INT NOT NULL,
		CONSTRAINT PK_CustomerAnalyticsPersonal PRIMARY KEY (CustomerAnalyticsPersonalID),
		CONSTRAINT FK_CustomerAnalyticsPersonal_Customer FOREIGN KEY (CustomerID) REFERENCES Customer(Id)
	)

	CREATE NONCLUSTERED INDEX IDX_CustomerAnalyticsPersonal ON CustomerAnalyticsPersonal(CustomerID) WHERE IsActive = 1
END
GO
