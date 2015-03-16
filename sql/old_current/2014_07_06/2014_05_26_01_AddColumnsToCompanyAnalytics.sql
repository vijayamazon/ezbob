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

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'Sic1980Code1' and Object_ID = Object_ID(N'CustomerAnalyticsCompany'))    
BEGIN
	ALTER TABLE CustomerAnalyticsCompany ADD Sic1980Code1 NVARCHAR(4)
END 
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'Sic1980Desc1' and Object_ID = Object_ID(N'CustomerAnalyticsCompany'))    
BEGIN
	ALTER TABLE CustomerAnalyticsCompany ADD Sic1980Desc1 NVARCHAR(75)
END 
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'Sic1992Code1' and Object_ID = Object_ID(N'CustomerAnalyticsCompany'))    
BEGIN
	ALTER TABLE CustomerAnalyticsCompany ADD Sic1992Code1 NVARCHAR(4)
END 
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'Sic1992Desc1' and Object_ID = Object_ID(N'CustomerAnalyticsCompany'))    
BEGIN
	ALTER TABLE CustomerAnalyticsCompany ADD Sic1992Desc1 NVARCHAR(75)
END 
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'AgeOfMostRecentCcj' and Object_ID = Object_ID(N'CustomerAnalyticsCompany'))    
BEGIN
	ALTER TABLE CustomerAnalyticsCompany ADD AgeOfMostRecentCcj INT
END 
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'NumOfCcjsInLast24Months' and Object_ID = Object_ID(N'CustomerAnalyticsCompany'))    
BEGIN
	ALTER TABLE CustomerAnalyticsCompany ADD NumOfCcjsInLast24Months INT
END 
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'SumOfCcjsInLast24Months' and Object_ID = Object_ID(N'CustomerAnalyticsCompany'))    
BEGIN
	ALTER TABLE CustomerAnalyticsCompany ADD SumOfCcjsInLast24Months INT
END 
GO

