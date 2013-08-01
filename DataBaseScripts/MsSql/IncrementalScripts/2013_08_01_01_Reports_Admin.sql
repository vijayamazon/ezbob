IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('ReportUsers') AND name = 'IsAdmin')
	ALTER TABLE ReportUsers ADD IsAdmin BIT NOT NULL CONSTRAINT DF_ReportUsersPIsAdmin DEFAULT (0)
GO

