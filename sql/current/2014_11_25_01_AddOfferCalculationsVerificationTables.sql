IF OBJECT_ID (N'dbo.OfferCalculationsSeekAV') IS NULL
BEGIN 
CREATE TABLE dbo.OfferCalculationsSeekAV
	(
	  Id                  INT IDENTITY NOT NULL
	, CustomerId          INT NOT NULL
	, CalculationTime     DATETIME
	, IsActive            BIT
	, Amount              INT
	, Medal               NVARCHAR (50)
	, ScenarioName        NVARCHAR (50)
	, Period              INT
	, IsEu                BIT
	, InterestRate        DECIMAL (18, 6)
	, SetupFee            DECIMAL (18, 6)
	, LoanType            NVARCHAR (30)
	, LoanSource          NVARCHAR (30)
	, Error               NVARCHAR (500)
	, CONSTRAINT PK_OfferCalculationsSeekAV PRIMARY KEY (Id)
	)
END 	
GO

IF OBJECT_ID('OfferCalculationsBoundariesAV') IS NULL
BEGIN 
CREATE TABLE OfferCalculationsBoundariesAV
	(
	  Id                  INT IDENTITY NOT NULL
	, CustomerId          INT NOT NULL
	, CalculationTime     DATETIME
	, IsActive            BIT
	, Amount              INT
	, Medal               NVARCHAR (50)
	, ScenarioName        NVARCHAR (50)
	, Period              INT
	, IsEu                BIT
	, InterestRate        DECIMAL (18, 6)
	, SetupFee            DECIMAL (18, 6)
	, LoanType            NVARCHAR (30)
	, LoanSource          NVARCHAR (30)
	, Error               NVARCHAR (500)
	, CONSTRAINT PK_OfferCalculationsBoundariesAV PRIMARY KEY (Id)
	)
END 	
GO