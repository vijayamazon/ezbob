IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'LoanId' and Object_ID = Object_ID(N'LoanLegal'))    
BEGIN
	ALTER TABLE LoanLegal DROP COLUMN LoanId
END 
GO

