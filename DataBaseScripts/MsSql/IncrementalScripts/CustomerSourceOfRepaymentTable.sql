IF OBJECT_ID ('dbo.CustomerSourceOfRepayment') IS NOT NULL
	DROP TABLE dbo.CustomerSourceOfRepayment
GO

CREATE TABLE dbo.CustomerSourceOfRepayment
	(
	  Id                                      INT IDENTITY NOT NULL
	, SourceOfRepayment                       NVARCHAR(300) NOT NULL
	, CONSTRAINT PK_CustomerSourceOfRepayment PRIMARY KEY (Id)
	)
GO

CREATE INDEX IX_CustomerSourceOfRepayment
	ON dbo.CustomerSourceOfRepayment (SourceOfRepayment)
GO

INSERT INTO CustomerSourceOfRepayment VALUES ('Mother') 
INSERT INTO CustomerSourceOfRepayment VALUES ('Other')
