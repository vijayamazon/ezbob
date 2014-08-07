IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'IsOwner' and Object_ID = Object_ID(N'CustomerPropertyStatuses'))    
BEGIN
	ALTER TABLE CustomerPropertyStatuses DROP COLUMN IsOwner
END
GO

