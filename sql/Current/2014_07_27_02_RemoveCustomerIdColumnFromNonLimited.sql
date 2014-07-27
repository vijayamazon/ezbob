IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'CustomerId' and Object_ID = Object_ID(N'ExperianNonLimitedResults'))
BEGIN
	ALTER TABLE ExperianNonLimitedResults DROP COLUMN CustomerId
END
GO

