IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'LastStartedMainStrategy' and Object_ID = Object_ID(N'Customer'))
BEGIN 
	ALTER TABLE Customer DROP COLUMN LastStartedMainStrategy
END 

IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'Eliminated' and Object_ID = Object_ID(N'Customer'))
BEGIN 
	ALTER TABLE Customer DROP COLUMN Eliminated
END 
GO
