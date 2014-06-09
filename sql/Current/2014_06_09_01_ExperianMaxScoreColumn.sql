IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'ExperianMaxScore' and Object_ID = Object_ID(N'MP_ExperianDataCache'))    
BEGIN
	ALTER TABLE MP_ExperianDataCache ADD ExperianMaxScore INT 
END 

GO 