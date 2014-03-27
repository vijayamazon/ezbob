IF OBJECT_ID('InsertFinancialAccount') IS NULL
	EXECUTE('CREATE PROCEDURE InsertFinancialAccount AS SELECT 1')
GO

ALTER PROCEDURE InsertFinancialAccount
	(@CustomerId INT,
	 @ServiceLogId INT,
	 @StartDate DATETIME,
	 @AccountStatus VARCHAR(10),
	 @DateType VARCHAR(25),
	 @SettlementOrDefaultDate DATETIME,
	 @LastUpdateDate DATETIME,
	 @StatusCode1 CHAR(1),
	 @StatusCode2 CHAR(1),
	 @StatusCode3 CHAR(1),
	 @StatusCode4 CHAR(1),
	 @StatusCode5 CHAR(1),
	 @StatusCode6 CHAR(1),
	 @StatusCode7 CHAR(1),
	 @StatusCode8 CHAR(1),
	 @StatusCode9 CHAR(1),
	 @StatusCode10 CHAR(1),
	 @StatusCode11 CHAR(1),
	 @StatusCode12 CHAR(1),
	 @CreditLimit INT,
	 @Balance INT,
	 @CurrentDefaultBalance INT,
	 @Status1To2 INT,
	 @StatusTo3 INT,
	 @WorstStatus CHAR(1),
	 @AccountType VARCHAR(3))
AS
BEGIN	
	INSERT INTO FinancialAccounts
	(ServiceLogId, CustomerId, StartDate, AccountStatus, DateType, SettlementOrDefaultDate, LastUpdateDate, StatusCode1, StatusCode2, StatusCode3, StatusCode4, StatusCode5, StatusCode6, StatusCode7, StatusCode8, StatusCode9, StatusCode10, StatusCode11, StatusCode12, CreditLimit, Balance, CurrentDefaultBalance, Status1To2, StatusTo3, WorstStatus, AccountType)
	VALUES
	(@ServiceLogId, @CustomerId, @StartDate, @AccountStatus, @DateType, @SettlementOrDefaultDate, @LastUpdateDate, @StatusCode1, @StatusCode2, @StatusCode3, @StatusCode4, @StatusCode5, @StatusCode6, @StatusCode7, @StatusCode8, @StatusCode9, @StatusCode10, @StatusCode11, @StatusCode12, @CreditLimit, @Balance, @CurrentDefaultBalance, @Status1To2, @StatusTo3, @WorstStatus, @AccountType)
END
GO
