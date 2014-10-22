IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'CostumeActionItem' and Object_ID = Object_ID(N'Customer'))    
BEGIN
	ALTER TABLE Customer ADD CostumeActionItem NVARCHAR(1000)
END
GO

