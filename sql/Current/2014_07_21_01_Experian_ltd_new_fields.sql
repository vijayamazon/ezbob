IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'First1992SICCode' AND id = OBJECT_ID('ExperianLtd'))
	ALTER TABLE ExperianLtd ADD First1992SICCode NVARCHAR(255) NULL
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'First1980SICCodeDescription' AND id = OBJECT_ID('ExperianLtd'))
	ALTER TABLE ExperianLtd ADD First1980SICCodeDescription NVARCHAR(255) NULL
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'First1980SICCode' AND id = OBJECT_ID('ExperianLtd'))
	ALTER TABLE ExperianLtd ADD First1980SICCode NVARCHAR(255) NULL
GO
