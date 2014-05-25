IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'EndSession' AND id = OBJECT_ID('CustomerSession'))
	ALTER TABLE CustomerSession ADD EndSession DATETIME NULL
GO
