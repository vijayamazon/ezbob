IF EXISTS (SELECT * FROM syscolumns WHERE name = 'AppId' AND id = OBJECT_ID('Security_Session'))
BEGIN
	ALTER TABLE Security_Session DROP CONSTRAINT FK_Security_Session_Security_Application
	ALTER TABLE Security_Session DROP COLUMN AppId
END
GO
