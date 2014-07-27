IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'AuthenticationIndexType' and Object_ID = Object_ID(N'AmlResults'))
BEGIN
	EXEC sp_rename 'AmlResults.AuthenticationIndexType', 'AuthenticationIndex', 'COLUMN'
	ALTER TABLE AmlResults ALTER COLUMN AuthenticationIndex INT
END
GO

