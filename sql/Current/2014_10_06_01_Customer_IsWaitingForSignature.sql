IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'IsWaitingForSignature' AND id = OBJECT_ID('Customer'))
	ALTER TABLE Customer ADD IsWaitingForSignature BIT NULL
GO
