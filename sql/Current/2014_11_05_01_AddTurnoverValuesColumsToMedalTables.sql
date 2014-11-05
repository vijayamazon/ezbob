IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'HmrcAnnualTurnover' and Object_ID = Object_ID(N'MedalCalculations'))    
BEGIN
	ALTER TABLE MedalCalculations ADD HmrcAnnualTurnover DECIMAL(18,6)
	ALTER TABLE MedalCalculations ADD BankAnnualTurnover DECIMAL(18,6)
	ALTER TABLE MedalCalculations ADD OnlineAnnualTurnover DECIMAL(18,6)	
	
	ALTER TABLE NewMedalComparison1 ADD HmrcAnnualTurnover DECIMAL(18,6)
	ALTER TABLE NewMedalComparison1 ADD BankAnnualTurnover DECIMAL(18,6)
	ALTER TABLE NewMedalComparison1 ADD OnlineAnnualTurnover DECIMAL(18,6)
	
	ALTER TABLE NewMedalComparison2 ADD HmrcAnnualTurnover DECIMAL(18,6)
	ALTER TABLE NewMedalComparison2 ADD BankAnnualTurnover DECIMAL(18,6)
	ALTER TABLE NewMedalComparison2 ADD OnlineAnnualTurnover DECIMAL(18,6)
END
GO





