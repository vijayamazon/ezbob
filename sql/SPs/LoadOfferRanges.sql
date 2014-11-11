IF OBJECT_ID('LoadOfferRanges') IS NULL
	EXECUTE('CREATE PROCEDURE LoadOfferRanges AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadOfferRanges
	@Amount INT,
	@MedalClassification NVARCHAR(50)
AS
BEGIN
	DECLARE
		@MinInterestRate DECIMAL(18,6),
		@MaxInterestRate DECIMAL(18,6),
		@MinSetupFee DECIMAL(18,6),
		@MaxSetupFee DECIMAL(18,6)
	
	SELECT
		@MinInterestRate = MinInterestRate,
		@MaxInterestRate = MaxInterestRate
	FROM
		OfferInterestRateRanges
	WHERE
		MedalClassification = @MedalClassification
	
	SELECT
		@MinSetupFee = MinSetupFee,
		@MaxSetupFee = MaxSetupFee
	FROM
		OfferSetupFeeRanges
	WHERE
		FromLoanAmount <= @Amount AND
		ToLoanAmount >= @Amount
		
	SELECT
		@MinInterestRate AS MinInterestRate,
		@MaxInterestRate AS MaxInterestRate,
		@MinSetupFee AS MinSetupFee,
		@MaxSetupFee AS MaxSetupFee
END
GO
