IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'ZooplaEstimateValue' and Object_ID = Object_ID(N'Zoopla'))    
BEGIN
	ALTER TABLE Zoopla ADD ZooplaEstimateValue INT
	
	DECLARE @Statement NVARCHAR(MAX)
	
	SET @Statement = 'UPDATE Zoopla SET ZooplaEstimateValue = 0'
	
	EXEC(@Statement)
END
GO

