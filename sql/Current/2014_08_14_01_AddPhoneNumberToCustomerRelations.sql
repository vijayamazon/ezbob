IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'PhoneNumber' and Object_ID = Object_ID(N'CustomerRelations'))    
BEGIN
	ALTER TABLE CustomerRelations ADD PhoneNumber NVARCHAR(50)
END
GO

