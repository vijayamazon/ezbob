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

INSERT INTO CustomerSourceOfRepayment VALUES ('Ongoing source of income') 
INSERT INTO CustomerSourceOfRepayment VALUES ('New sources of income')
INSERT INTO CustomerSourceOfRepayment VALUES ('New debt')
INSERT INTO CustomerSourceOfRepayment VALUES ('Sale of fixed assets')
INSERT INTO CustomerSourceOfRepayment VALUES ('Other')
