IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'NumOfHmrcMps' and Object_ID = Object_ID(N'MedalCalculations'))    
BEGIN
	ALTER TABLE MedalCalculations ADD NumOfHmrcMps INT
	ALTER TABLE MedalCalculations ADD ZooplaValue INT	
	ALTER TABLE MedalCalculations ADD EarliestHmrcLastUpdateDate DATETIME
	ALTER TABLE MedalCalculations ADD EarliestYodleeLastUpdateDate DATETIME	
	ALTER TABLE MedalCalculations ADD AmazonPositiveFeedbacks INT
	ALTER TABLE MedalCalculations ADD EbayPositiveFeedbacks INT
	ALTER TABLE MedalCalculations ADD NumberOfPaypalPositiveTransactions INT	
	ALTER TABLE MedalCalculations ADD MortgageBalance DECIMAL(18,6)
END
GO
