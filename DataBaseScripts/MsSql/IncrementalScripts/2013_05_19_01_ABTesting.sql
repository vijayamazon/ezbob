IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'ABTesting' AND id = OBJECT_ID('Customer'))
BEGIN
	ALTER TABLE Customer ADD ABTesting NVARCHAR(512) NULL
END
GO
