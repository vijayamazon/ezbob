IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'IsCurrent' and Object_ID = Object_ID(N'CustomerPhones'))    
BEGIN
	ALTER TABLE CustomerPhones ADD IsCurrent BIT
	
	DECLARE @Statement NVARCHAR(MAX)
	
	SET @Statement = 'UPDATE CustomerPhones SET IsCurrent = 1'
	
	EXEC(@Statement)
END
GO

