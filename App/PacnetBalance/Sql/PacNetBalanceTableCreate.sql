IF OBJECT_ID ('dbo.PacNetBalance') IS NOT NULL
BEGIN
--		DROP TABLE dbo.PacNetBalance
	PRINT 'PacNetBalance exists'
END
ELSE
BEGIN
CREATE TABLE dbo.PacNetBalance
	(
	  Id             INT IDENTITY NOT NULL
	, [Date]         DATETIME
	, Amount         FLOAT
	, Fees           FLOAT
	, CurrentBalance FLOAT
	, IsCredit       BIT DEFAULT ((0))
	)
END
GO

