IF OBJECT_ID ('dbo.PacNetBalance') IS NOT NULL
	DROP TABLE dbo.PacNetBalance
GO

CREATE TABLE dbo.PacNetBalance
	(
	  Id             INT IDENTITY NOT NULL
	, [Date]         DATETIME
	, Amount         FLOAT
	, Fees           FLOAT
	, CurrentBalance FLOAT
	, IsCredit       BIT DEFAULT ((0))
	)
GO

