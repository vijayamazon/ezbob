IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'IsOwnerAccordingToLandRegistry' and Object_ID = Object_ID(N'CustomerAddress'))    
BEGIN
	ALTER TABLE CustomerAddress ADD IsOwnerAccordingToLandRegistry BIT
	
	DECLARE @Statement NVARCHAR(MAX)
	
	SET @Statement = 'UPDATE CustomerAddress SET IsOwnerAccordingToLandRegistry = 0'
	
	EXEC(@Statement)
END
GO

