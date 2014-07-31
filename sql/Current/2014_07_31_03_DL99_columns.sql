IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'FinDirLoans' AND id = OBJECT_ID('ExperianLtdDL99'))
	ALTER TABLE ExperianLtdDL99 ADD FinDirLoans DECIMAL(18, 6) NULL
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'FinLbltsDirLoans' AND id = OBJECT_ID('ExperianLtdDL99'))
	ALTER TABLE ExperianLtdDL99 ADD FinLbltsDirLoans DECIMAL(18, 6) NULL
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'CurrDirLoans' AND id = OBJECT_ID('ExperianLtdDL99'))
	ALTER TABLE ExperianLtdDL99 ADD CurrDirLoans DECIMAL(18, 6) NULL
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'TotalCurrAssets' AND id = OBJECT_ID('ExperianLtdDL99'))
	ALTER TABLE ExperianLtdDL99 ADD TotalCurrAssets DECIMAL(18, 6) NULL
GO
