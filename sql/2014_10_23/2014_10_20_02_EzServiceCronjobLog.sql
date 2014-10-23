IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'ActionID' AND id = OBJECT_ID('EzServiceCronjobLog'))
	ALTER TABLE EzServiceCronjobLog ADD ActionID UNIQUEIDENTIFIER NULL
GO
