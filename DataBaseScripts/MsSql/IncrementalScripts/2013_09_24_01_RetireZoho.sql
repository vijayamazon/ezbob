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
IF EXISTS (SELECT name FROM sysindexes WHERE name = 'IX_LoanAgreement_Loan') 
BEGIN
	DROP INDEX LoanAgreement.IX_LoanAgreement_Loan
END
GO 

IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'ZohoId' and Object_ID = Object_ID(N'LoanAgreement'))    
BEGIN
	ALTER TABLE LoanAgreement DROP COLUMN ZohoId
END
GO

CREATE NONCLUSTERED INDEX [IX_LoanAgreement_Loan] ON [dbo].[LoanAgreement] 
(
	[LoanId] ASC
)
INCLUDE ( [Name],
[Template],
[FilePath]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
