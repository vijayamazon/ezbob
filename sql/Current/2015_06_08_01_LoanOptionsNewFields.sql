IF NOT EXISTS (SELECT * FROM syscolumns WHERE id=object_id('LoanOptions') AND name='AutoInterest')
BEGIN
	ALTER TABLE LoanOptions ADD AutoInterest BIT NOT NULL DEFAULT(1)
   	ALTER TABLE LoanOptions ADD StopAutoChargeDate DATETIME 
   	ALTER TABLE LoanOptions ADD StopLateFeeDate DATETIME
	ALTER TABLE LoanOptions ADD StopAutoInterestDate DATETIME
END
GO 