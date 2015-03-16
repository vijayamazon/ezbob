IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('Customer') AND name = 'GoogleCookie')
	ALTER TABLE Customer ADD GoogleCookie NVARCHAR(300)
GO
