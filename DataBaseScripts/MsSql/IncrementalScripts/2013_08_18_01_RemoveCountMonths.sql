IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'CountMonths' and Object_ID = Object_ID(N'MP_AnalyisisFunctionValues'))    
BEGIN
	ALTER TABLE MP_AnalyisisFunctionValues DROP COLUMN CountMonths
END
GO
