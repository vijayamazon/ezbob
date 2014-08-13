IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'CustomerId' and Object_ID = Object_ID(N'AmlResults'))    
BEGIN
	ALTER TABLE AmlResults DROP COLUMN CustomerId
END
GO

