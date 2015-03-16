IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'AmlDescription' and Object_ID = Object_ID(N'Customer'))    
BEGIN
	ALTER TABLE Customer ADD AmlDescription NVARCHAR(200)
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'AmlScore' and Object_ID = Object_ID(N'Customer'))    
BEGIN
	ALTER TABLE Customer ADD AmlScore INT
END
GO
