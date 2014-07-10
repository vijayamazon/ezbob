IF OBJECT_ID('InsertNonLimitedResult') IS NULL
	EXECUTE('CREATE PROCEDURE InsertNonLimitedResult AS SELECT 1')
GO

ALTER PROCEDURE InsertNonLimitedResult
	(@CustomerId INT,
	 @RefNumber NVARCHAR(50),
	 @ServiceLogId INT,
	 @Created DATETIME,		
	 @BusinessName NVARCHAR(75),
	 @Address1 NVARCHAR(30),
	 @Address2 NVARCHAR(30),
	 @Address3 NVARCHAR(30),
	 @Address4 NVARCHAR(30),
	 @Address5 NVARCHAR(30),
	 @Postcode NVARCHAR(8),		
	 @IncorporationDate DATETIME,
	 @RiskScore INT,
	 @Score INT,
	 @CreditLimit INT,		
	 @AgeOfMostRecentCcj INT,
	 @NumOfCcjsInLast12Months INT,
	 @NumOfCcjsIn13To24Months INT,
	 @SumOfCcjsInLast12Months INT,
	 @SumOfCcjsIn13To24Months INT,
	 @NumOfCcjsInLast24Months INT,
	 @NumOfAssociatedCcjsInLast24Months INT,
	 @SumOfCcjsInLast24Months INT,
	 @SumOfAssociatedCcjsInLast24Months INT)
AS
BEGIN
	SET NOCOUNT ON;
	
	UPDATE NonLimitedResults SET IsActive = 0 WHERE CustomerId = @CustomerId AND RefNumber = @RefNumber
	
	INSERT INTO NonLimitedResults
		(CustomerId, RefNumber, ServiceLogId, Created, BusinessName, Address1, Address2, Address3, Address4, Address5, Postcode,	IncorporationDate, RiskScore, Score, CreditLimit, AgeOfMostRecentCcj, NumOfCcjsInLast12Months, NumOfCcjsIn13To24Months, SumOfCcjsInLast12Months, SumOfCcjsIn13To24Months, NumOfCcjsInLast24Months, NumOfAssociatedCcjsInLast24Months, SumOfCcjsInLast24Months, SumOfAssociatedCcjsInLast24Months, IsActive)
	VALUES
		(@CustomerId, @RefNumber, @ServiceLogId, @Created, @BusinessName, @Address1, @Address2, @Address3, @Address4, @Address5, @Postcode,	@IncorporationDate, @RiskScore, @Score, @CreditLimit, @AgeOfMostRecentCcj, @NumOfCcjsInLast12Months, @NumOfCcjsIn13To24Months, @SumOfCcjsInLast12Months, @SumOfCcjsIn13To24Months, @NumOfCcjsInLast24Months, @NumOfAssociatedCcjsInLast24Months, @SumOfCcjsInLast24Months, @SumOfAssociatedCcjsInLast24Months, 1)
END
GO
