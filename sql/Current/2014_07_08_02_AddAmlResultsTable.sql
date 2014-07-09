IF OBJECT_ID('AmlResults') IS NULL
BEGIN
	CREATE TABLE AmlResults (
		Id INT IDENTITY NOT NULL,
		[Key] NVARCHAR(500),
		CustomerId INT,
		ServiceLogId INT,
		Created DATETIME,
		AuthenticationDecision NVARCHAR(20),
		AuthenticationIndexType DECIMAL(18, 6),
		AuthIndexText NVARCHAR(500),
		NumPrimDataItems DECIMAL(18, 6),
		NumPrimDataSources DECIMAL(18, 6),
		NumSecDataItems DECIMAL(18, 6),
		StartDateOldestPrim NVARCHAR(500),
		StartDateOldestSec NVARCHAR(500),
		Error NVARCHAR(MAX),		
		IsActive BIT
	)
END
GO

IF OBJECT_ID('AmlResultsHighRiskRules') IS NULL
BEGIN
	CREATE TABLE AmlResultsHighRiskRules (
		Id INT IDENTITY NOT NULL,
		AmlResultId INT,
		RuleId NVARCHAR(10),
		RuleText NVARCHAR(500)
	)
END
GO
