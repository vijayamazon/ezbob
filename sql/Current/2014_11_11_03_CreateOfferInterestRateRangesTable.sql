IF object_id('OfferInterestRateRanges') IS NULL
BEGIN
	CREATE TABLE OfferInterestRateRanges(
	    Id INT NOT NULL IDENTITY(1,1)
	   ,MedalClassification NVARCHAR(50)
	   ,MinInterestRate DECIMAL(18, 6)
	   ,MaxInterestRate DECIMAL(18, 6)
	   ,CONSTRAINT PK_OfferInterestRateRanges PRIMARY KEY (Id)
	)
	
	INSERT INTO OfferInterestRateRanges VALUES('Silver', 3, 5)
	INSERT INTO OfferInterestRateRanges VALUES('Gold', 2.75, 4.5)
	INSERT INTO OfferInterestRateRanges VALUES('Platinum', 2.5, 4)
	INSERT INTO OfferInterestRateRanges VALUES('Diamond', 2, 3.5)
END
GO
