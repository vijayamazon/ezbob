IF OBJECT_ID('AmlResults') IS NOT NULL
BEGIN
	IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'Key' and Object_ID = Object_ID(N'AmlResults'))
	BEGIN
		EXEC sp_rename 'AmlResults.Key', 'LookupKey', 'COLUMN'
	END
END
GO
