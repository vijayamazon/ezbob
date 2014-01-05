IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'CapitalExpenditure' and Object_ID = Object_ID(N'Customer'))    
BEGIN
	ALTER TABLE Customer ADD CapitalExpenditure DECIMAL(18) DEFAULT(0)
END 
GO
