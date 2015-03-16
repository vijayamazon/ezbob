IF object_id('OfferSetupFeeRanges') IS NULL
BEGIN
	CREATE TABLE OfferSetupFeeRanges(
	    Id INT NOT NULL IDENTITY(1,1)
	   ,LoanSizeName NVARCHAR(50)
	   ,FromLoanAmount INT
	   ,ToLoanAmount INT
	   ,MinSetupFee DECIMAL(18, 6)
	   ,MaxSetupFee DECIMAL(18, 6)
	   ,CONSTRAINT PK_OfferSetupFeeRanges PRIMARY KEY (Id)
	)
	
	INSERT INTO OfferSetupFeeRanges VALUES('Micro', 0, 4999, 7.5, 10)
	INSERT INTO OfferSetupFeeRanges VALUES('Small', 5000, 19999, 5, 10)
	INSERT INTO OfferSetupFeeRanges VALUES('Average', 20000, 34999, 4, 7.5)
	INSERT INTO OfferSetupFeeRanges VALUES('Large', 35000, 44999, 3, 6)
	INSERT INTO OfferSetupFeeRanges VALUES('XL', 45000, 50000, 2, 5)
END
GO
