IF OBJECT_ID ('dbo.MP_SagePaymentStatus') IS NOT NULL
	DROP TABLE dbo.MP_SagePaymentStatus
GO

CREATE TABLE dbo.MP_SagePaymentStatus
	(
     Id INT IDENTITY NOT NULL
    ,SageId INT NOT NULL
    ,name NVARCHAR(250)
	
    ,CONSTRAINT PK_MP_SagePaymentStatus PRIMARY KEY (Id)
	)
GO

