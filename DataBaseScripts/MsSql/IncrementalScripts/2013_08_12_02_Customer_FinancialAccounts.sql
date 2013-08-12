IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'FinancialAccounts' and Object_ID = Object_ID(N'Customer'))
BEGIN
ALTER TABLE dbo.Customer ADD
	FinancialAccounts int NOT NULL CONSTRAINT DF_Customer_FinancialAccounts DEFAULT 0
END
GO
