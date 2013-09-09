IF OBJECT_ID ('dbo.ApprovalsWithoutAML') IS NOT NULL
	DROP TABLE dbo.ApprovalsWithoutAML
GO
CREATE TABLE dbo.ApprovalsWithoutAML
	(
	  Id INT IDENTITY NOT NULL
	, CustomerId INT NOT NULL
	, Username NVARCHAR(100)
	, [Timestamp] DATETIME
	, DoNotShowAgain BIT
	, CONSTRAINT PK_ApprovalsWithoutAML PRIMARY KEY (Id)
	)
	
GO
 