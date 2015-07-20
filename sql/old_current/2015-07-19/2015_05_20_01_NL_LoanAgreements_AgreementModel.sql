SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('NL_LoanAgreements') AND name = 'AgreementModel')
BEGIN
	ALTER TABLE NL_LoanAgreements DROP COLUMN TimestampCounter

	ALTER TABLE NL_LoanAgreements ADD AgreementModel NVARCHAR(MAX) NULL

	ALTER TABLE NL_LoanAgreements ADD TimestampCounter ROWVERSION
END
GO
