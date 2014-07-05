IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'IsEncrypted' AND id = OBJECT_ID('ConfigurationVariables'))
	ALTER TABLE ConfigurationVariables ADD IsEncrypted BIT NULL
GO
