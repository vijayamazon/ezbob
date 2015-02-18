UPDATE dbo.LoanSource
SET DefaultRepaymentPeriod = 15, MaxEmployeeCount = 250, MaxAnnualTurnover = 40000000
WHERE LoanSourceName = 'COSME'
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE name='COSMEAgreementAgreed' AND id = OBJECT_ID('LoanLegal'))
BEGIN
	ALTER TABLE LoanLegal ADD COSMEAgreementAgreed BIT NOT NULL DEFAULT(0)
END
GO