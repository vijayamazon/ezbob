IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'PropertyStatusId' and Object_ID = Object_ID(N'Customer'))
BEGIN
	ALTER TABLE Customer ADD PropertyStatusId INT
END
GO

