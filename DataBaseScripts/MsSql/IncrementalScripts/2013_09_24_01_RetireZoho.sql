IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'ZohoId' and Object_ID = Object_ID(N'CashRequest'))    
BEGIN
	ALTER TABLE CashRequest DROP COLUMN ZohoId
END
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'ZohoId' and Object_ID = Object_ID(N'Customer'))    
BEGIN
	ALTER TABLE Customer DROP COLUMN ZohoId
END
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'ZohoId' and Object_ID = Object_ID(N'Loan'))    
BEGIN
	ALTER TABLE Loan DROP COLUMN ZohoId
END
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'ZohoId' and Object_ID = Object_ID(N'LoanAgreement'))    
BEGIN
	ALTER TABLE LoanAgreement DROP COLUMN ZohoId
END
GO