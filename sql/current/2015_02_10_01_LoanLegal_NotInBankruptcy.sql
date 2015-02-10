SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('LoanLegal') AND name = 'NotInBankruptcy')
	ALTER TABLE LoanLegal ADD NotInBankruptcy BIT NULL
GO