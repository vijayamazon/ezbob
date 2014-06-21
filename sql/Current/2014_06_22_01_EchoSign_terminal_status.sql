IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'IsTerminal' AND id = OBJECT_ID('EsignAgreementStatus'))
	ALTER TABLE EsignAgreementStatus ADD IsTerminal BIT NOT NULL CONSTRAINT DF_EsignAgreementStatus_Termianl DEFAULT (0)
GO

UPDATE EsignAgreementStatus SET
	IsTerminal = 1
WHERE
	StatusID IN (2, 4, 5, 6, 7, 8)
GO
