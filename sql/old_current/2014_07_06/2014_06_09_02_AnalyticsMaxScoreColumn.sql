IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'MaxScore' and Object_ID = Object_ID(N'CustomerAnalyticsCompany'))    
BEGIN
	ALTER TABLE CustomerAnalyticsCompany ADD MaxScore INT 
END 

GO 