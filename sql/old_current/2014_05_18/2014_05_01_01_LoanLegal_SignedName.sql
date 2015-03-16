IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'SignedName' AND id = OBJECT_ID('LoanLegal'))
	ALTER TABLE LoanLegal ADD SignedName NVARCHAR(250) NULL
GO
