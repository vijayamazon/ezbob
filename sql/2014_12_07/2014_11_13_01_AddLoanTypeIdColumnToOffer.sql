IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'LoanTypeId' and Object_ID = Object_ID(N'OfferCalculations'))    
BEGIN
	ALTER TABLE OfferCalculations ADD LoanTypeId INT
END
GO
