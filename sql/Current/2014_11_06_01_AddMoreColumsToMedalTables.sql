IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'CalculationTime' and Object_ID = Object_ID(N'MedalCalculations'))    
BEGIN
	ALTER TABLE MedalCalculations ADD CalculationTime DATETIME
	ALTER TABLE MedalCalculations ADD FirstRepaymentDatePassed BIT
	ALTER TABLE MedalCalculations ADD OfferedLoanAmount INT
END
GO
