IF OBJECT_ID ('dbo.CustomerReason') IS NOT NULL
	DROP TABLE dbo.CustomerReason
GO

CREATE TABLE dbo.CustomerReason
	(
	  Id                           INT IDENTITY NOT NULL
	, Reason                       NVARCHAR(300) NOT NULL
	, CONSTRAINT PK_CustomerReason PRIMARY KEY (Id)
	)
GO

CREATE INDEX IX_CustomerReason
	ON dbo.CustomerReason (Reason)
GO

INSERT INTO CustomerReason VALUES ('No Food') 
INSERT INTO CustomerReason VALUES ('Other')
