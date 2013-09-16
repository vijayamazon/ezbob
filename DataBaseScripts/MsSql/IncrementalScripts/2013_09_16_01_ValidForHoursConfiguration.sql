IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name = 'OfferValidForHours')
BEGIN
	INSERT INTO ConfigurationVariables VALUES ('OfferValidForHours', '72', 'Number of hours offer is valid for by default')
END
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'OfferStart' and Object_ID = Object_ID(N'CashRequests'))    
BEGIN
	ALTER TABLE CashRequests DROP COLUMN OfferStart
END
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'OfferValidUntil' and Object_ID = Object_ID(N'CashRequests'))    
BEGIN
	ALTER TABLE CashRequests DROP COLUMN OfferValidUntil
END
GO