IF OBJECT_ID ('dbo.YodleeBanks') IS NOT NULL
BEGIN
	PRINT 'YodleeBanks table already exist'
END
ELSE
BEGIN
	CREATE TABLE dbo.YodleeBanks
	(
		Id INT IDENTITY NOT NULL
		,Name NVARCHAR(300)
	   ,CONSTRAINT PK_YodleeBanks PRIMARY KEY (Id)
	)
END
GO

