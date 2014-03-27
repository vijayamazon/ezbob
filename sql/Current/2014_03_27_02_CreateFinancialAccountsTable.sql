IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FinancialAccounts]') AND type IN (N'U'))
BEGIN
	CREATE TABLE FinancialAccounts (
		Id INT IDENTITY NOT NULL 
	  , ServiceLogId INT
	  , CustomerId INT	  
	  , StartDate DATETIME
	  , AccountStatus VARCHAR(10)
	  , DateType VARCHAR(25)
	  , SettlementOrDefaultDate DATETIME
	  , LastUpdateDate DATETIME
	  , StatusCode1 CHAR(1)
	  , StatusCode2 CHAR(1)
	  , StatusCode3 CHAR(1)
	  , StatusCode4 CHAR(1)
	  , StatusCode5 CHAR(1)
	  , StatusCode6 CHAR(1)
	  , StatusCode7 CHAR(1)
	  , StatusCode8 CHAR(1)
	  , StatusCode9 CHAR(1)
	  , StatusCode10 CHAR(1)
	  , StatusCode11 CHAR(1)
	  , StatusCode12 CHAR(1)
	  , CreditLimit INT
	  , Balance INT
	  , CurrentDefaultBalance INT
	  , Status1To2 INT
	  , StatusTo3 INT
	  , WorstStatus CHAR(1)
	  , AccountType VARCHAR(3)
	)
END
GO
