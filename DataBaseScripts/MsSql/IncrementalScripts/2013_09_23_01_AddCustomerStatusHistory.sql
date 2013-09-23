IF OBJECT_ID ('dbo.CustomerStatusHistory') IS NOT NULL
	DROP TABLE dbo.CustomerStatusHistory
GO
CREATE TABLE dbo.CustomerStatusHistory
	(
	  Id INT IDENTITY
	, Username NVARCHAR(100)
	, TimeStamp DATETIME
	, CustomerId INT
	, PreviousStatus INT
	, NewStatus INT
	, CONSTRAINT PK_CustomerStatusHistory PRIMARY KEY (Id)
	)
GO