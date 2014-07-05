IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'FirstVisitTime' AND id = OBJECT_ID('Customer'))
	ALTER TABLE Customer ADD FirstVisitTime NVARCHAR(64) NULL
GO
