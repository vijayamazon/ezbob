IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'IsShareholder' AND id = OBJECT_ID('Director'))
	ALTER TABLE Director ADD IsShareholder BIT NULL
GO
