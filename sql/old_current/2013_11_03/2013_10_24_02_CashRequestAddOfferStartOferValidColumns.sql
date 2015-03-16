IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'OfferStart' and Object_ID = Object_ID(N'CashRequests'))    
BEGIN
	ALTER TABLE CashRequests ADD OfferStart DATETIME
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'OfferValidUntil' and Object_ID = Object_ID(N'CashRequests'))    
BEGIN
	ALTER TABLE CashRequests ADD OfferValidUntil DATETIME
END
GO

UPDATE CashRequests SET OfferStart = CreationDate WHERE OfferStart IS NULL
GO 
UPDATE CashRequests SET OfferValidUntil = dateadd(day, 1, CreationDate) WHERE OfferValidUntil IS NULL
GO 