IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'CurrentBalanceSum' and Object_ID = Object_ID(N'MP_ExperianDataCache'))    
BEGIN
	ALTER TABLE MP_ExperianDataCache ADD CurrentBalanceSum INT DEFAULT(0)
END 
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'CurrentBalanceSum' and Object_ID = Object_ID(N'CustomerAnalyticsCompany'))    
BEGIN
	ALTER TABLE CustomerAnalyticsCompany ADD CurrentBalanceSum INT DEFAULT(0)
END 
GO

