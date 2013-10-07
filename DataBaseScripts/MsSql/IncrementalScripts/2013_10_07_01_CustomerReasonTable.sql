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


INSERT INTO CustomerReason VALUES ('Bridging temporary cash shortage due to slow collections')
INSERT INTO CustomerReason VALUES ('Bridging temporary cash shortage due to VAT')
INSERT INTO CustomerReason VALUES ('Bridging temporary cash shortage due to other unexpected expenses')
INSERT INTO CustomerReason VALUES ('Bridging seasonality')
INSERT INTO CustomerReason VALUES ('Capital expenditure')
INSERT INTO CustomerReason VALUES ('Debt repayment')
INSERT INTO CustomerReason VALUES ('Other')
