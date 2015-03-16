IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'Error' and Object_ID = Object_ID(N'OfflineScoring'))    
BEGIN
	ALTER TABLE OfflineScoring ADD Error NVARCHAR (500)
END
GO

