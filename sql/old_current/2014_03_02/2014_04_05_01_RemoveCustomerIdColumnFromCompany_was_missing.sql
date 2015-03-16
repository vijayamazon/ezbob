IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'CustomerId' and Object_ID = Object_ID(N'Company'))
BEGIN 
	
	ALTER TABLE Company DROP FK_Company_Customer
	ALTER TABLE Company DROP COLUMN CustomerId
END 
GO
