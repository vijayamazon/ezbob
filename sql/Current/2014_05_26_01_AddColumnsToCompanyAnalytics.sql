IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'TangibleEquity' and Object_ID = Object_ID(N'CustomerAnalyticsCompany'))    
BEGIN
	ALTER TABLE CustomerAnalyticsCompany ADD TangibleEquity DECIMAL(18,6) DEFAULT(0)
END 
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'AdjustedProfit' and Object_ID = Object_ID(N'CustomerAnalyticsCompany'))    
BEGIN
	ALTER TABLE CustomerAnalyticsCompany ADD AdjustedProfit DECIMAL(18,6) DEFAULT(0)
END 
GO

