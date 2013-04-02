CREATE TABLE dbo.PayPointCard
	(
	Id int NOT NULL,
	CustomerId int NOT NULL,
	DateAdded datetime NOT NULL,
	TransactionId nvarchar(250) NULL,
	CardNo nvarchar(50) NULL,
	ExpireDate datetime NULL,
	ExpireDateString nvarchar(50) NULL
	)  ON [PRIMARY]
GO

ALTER TABLE dbo.Loan ADD
	PayPointCardId int NULL
GO