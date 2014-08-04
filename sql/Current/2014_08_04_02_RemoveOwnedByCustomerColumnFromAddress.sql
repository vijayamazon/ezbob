IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'OwnedByCustomer' and Object_ID = Object_ID(N'CustomerAddress'))    
BEGIN
	ALTER TABLE CustomerAddress DROP COLUMN OwnedByCustomer
END
GO

