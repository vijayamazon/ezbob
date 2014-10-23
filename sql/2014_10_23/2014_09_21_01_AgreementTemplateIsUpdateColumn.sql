IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'IsUpdate' and Object_ID = Object_ID(N'LoanAgreementTemplate'))    
	ALTER TABLE LoanAgreementTemplate ADD IsUpdate BIT NOT NULL DEFAULT(0)
	
GO
