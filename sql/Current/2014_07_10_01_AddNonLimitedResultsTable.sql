IF OBJECT_ID('NonLimitedResults') IS NULL
BEGIN
	CREATE TABLE NonLimitedResults (
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

