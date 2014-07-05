IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'EzPassword' AND id = OBJECT_ID('Security_User'))
	ALTER TABLE Security_User ADD EzPassword varchar(255) NULL
GO
