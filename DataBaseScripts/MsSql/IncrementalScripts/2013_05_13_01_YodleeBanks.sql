IF OBJECT_ID ('dbo.YodleeBanks') IS NOT NULL
	DROP TABLE dbo.YodleeBanks
GO

CREATE TABLE dbo.YodleeBanks
(
	Id INT IDENTITY NOT NULL
	,Name NVARCHAR(300)
  , CONSTRAINT PK_YodleeBanks PRIMARY KEY (Id)
 )
GO

