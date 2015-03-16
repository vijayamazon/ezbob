IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'Created' AND id = OBJECT_ID('ExperianLtd'))
	ALTER TABLE ExperianLtd ADD Created DATETIME NOT NULL CONSTRAINT DF_ExperianLtd_Created DEFAULT (GETUTCDATE())
GO
