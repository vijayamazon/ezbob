SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('AV_OfferInterestRateRange') IS NULL
	EXECUTE('CREATE PROCEDURE AV_OfferInterestRateRange AS SELECT 1')
GO

ALTER PROCEDURE AV_OfferInterestRateRange
@Medal NVARCHAR(20)
AS
BEGIN
	SELECT MinInterestRate, MaxInterestRate 
	FROM OfferInterestRateRanges 
	WHERE MedalClassification=@Medal
END
GO
