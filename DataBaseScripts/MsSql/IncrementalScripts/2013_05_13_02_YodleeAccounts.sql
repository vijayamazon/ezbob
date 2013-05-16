IF OBJECT_ID ('dbo.YodleeAccounts') IS NOT NULL
BEGIN
	PRINT 'YodleeAccounts table already exist'
END
ELSE
BEGIN
	CREATE TABLE dbo.YodleeAccounts
	(
		Id INT IDENTITY NOT NULL
		,CustomerId INT NOT NULL
		,BankId INT NOT NULL
		,Username NVARCHAR(300)
		,Password NVARCHAR(300) -- consider keepting an encrypted password
		,CreationDate DATETIME
	    ,CONSTRAINT PK_YodleeAccounts PRIMARY KEY (Id)
	    ,CONSTRAINT FK_YodleeAccounts_Customer FOREIGN KEY (CustomerId) REFERENCES dbo.Customer (Id)
	    ,CONSTRAINT FK_YodleeAccounts_YodleeBanks FOREIGN KEY (BankId) REFERENCES dbo.YodleeBanks (Id)
	)
END
GO

