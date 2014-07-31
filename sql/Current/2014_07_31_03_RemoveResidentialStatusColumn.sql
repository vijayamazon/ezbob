IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'ResidentialStatus' and Object_ID = Object_ID(N'Customer'))
BEGIN 
	ALTER TABLE Customer DROP COLUMN ResidentialStatus
END 
GO
