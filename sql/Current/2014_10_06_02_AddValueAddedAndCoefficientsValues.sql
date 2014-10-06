IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'FreeCashFlowValue' and Object_ID = Object_ID(N'OfflineScoring'))    
BEGIN
	ALTER TABLE OfflineScoring ADD FreeCashFlowValue DECIMAL(18,6)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'TangibleEquityValue' and Object_ID = Object_ID(N'OfflineScoring'))    
BEGIN
	ALTER TABLE OfflineScoring ADD TangibleEquityValue DECIMAL(18,6)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'ValueAdded' and Object_ID = Object_ID(N'OfflineScoring'))    
BEGIN
	ALTER TABLE OfflineScoring ADD ValueAdded DECIMAL(18,6)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'BasedOnHmrcValues' and Object_ID = Object_ID(N'OfflineScoring'))    
BEGIN
	ALTER TABLE OfflineScoring ADD BasedOnHmrcValues BIT
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'FreeCashFlowValue' and Object_ID = Object_ID(N'NewMedalComparison1'))    
BEGIN
	ALTER TABLE NewMedalComparison1 ADD FreeCashFlowValue DECIMAL(18,6)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'TangibleEquityValue' and Object_ID = Object_ID(N'NewMedalComparison1'))    
BEGIN
	ALTER TABLE NewMedalComparison1 ADD TangibleEquityValue DECIMAL(18,6)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'ValueAdded' and Object_ID = Object_ID(N'NewMedalComparison1'))    
BEGIN
	ALTER TABLE NewMedalComparison1 ADD ValueAdded DECIMAL(18,6)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'BasedOnHmrcValues' and Object_ID = Object_ID(N'NewMedalComparison1'))    
BEGIN
	ALTER TABLE NewMedalComparison1 ADD BasedOnHmrcValues BIT
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'FreeCashFlowValue' and Object_ID = Object_ID(N'NewMedalComparison2'))    
BEGIN
	ALTER TABLE NewMedalComparison2 ADD FreeCashFlowValue DECIMAL(18,6)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'TangibleEquityValue' and Object_ID = Object_ID(N'NewMedalComparison2'))    
BEGIN
	ALTER TABLE NewMedalComparison2 ADD TangibleEquityValue DECIMAL(18,6)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'ValueAdded' and Object_ID = Object_ID(N'NewMedalComparison2'))    
BEGIN
	ALTER TABLE NewMedalComparison2 ADD ValueAdded DECIMAL(18,6)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'BasedOnHmrcValues' and Object_ID = Object_ID(N'NewMedalComparison2'))    
BEGIN
	ALTER TABLE NewMedalComparison2 ADD BasedOnHmrcValues BIT
END
GO
