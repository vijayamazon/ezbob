IF OBJECT_ID('ExperianNonLimitedResults') IS NULL
BEGIN
	CREATE TABLE ExperianNonLimitedResults (
		Id INT IDENTITY NOT NULL,
		CustomerId INT,
		RefNumber NVARCHAR(50),
		ServiceLogId INT,
		Created DATETIME,		
		BusinessName NVARCHAR(75),
		Address1 NVARCHAR(30),
		Address2 NVARCHAR(30),
		Address3 NVARCHAR(30),
		Address4 NVARCHAR(30),
		Address5 NVARCHAR(30),
		Postcode NVARCHAR(8),		
		IncorporationDate DATETIME,
		RiskScore INT,
		Score INT,
		CreditLimit INT,		
		AgeOfMostRecentCcj INT,
		NumOfCcjsInLast12Months INT,
		NumOfCcjsIn13To24Months INT,
		SumOfCcjsInLast12Months INT,
		SumOfCcjsIn13To24Months INT,
		NumOfCcjsInLast24Months INT,
		NumOfAssociatedCcjsInLast24Months INT,
		SumOfCcjsInLast24Months INT,
		SumOfAssociatedCcjsInLast24Months INT,		
		IsActive BIT
	)
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'Errors' and Object_ID = Object_ID(N'ExperianNonLimitedResults'))
BEGIN 
	ALTER TABLE ExperianNonLimitedResults 
	ADD Errors NVARCHAR(MAX)
END 
GO
