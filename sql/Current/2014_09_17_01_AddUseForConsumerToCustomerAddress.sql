IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'UseForConsumer' and Object_ID = Object_ID(N'CustomerAddress'))    
BEGIN
	ALTER TABLE CustomerAddress ADD UseForConsumer BIT
	
	DECLARE @Statement NVARCHAR(MAX)
	
	SET @Statement = 'UPDATE CustomerAddress SET UseForConsumer = 0'
	
	EXEC(@Statement)
END
GO

