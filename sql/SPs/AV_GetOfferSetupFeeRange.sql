
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('AV_GetOfferSetupFeeRange') IS NULL
	EXECUTE('CREATE PROCEDURE AV_GetOfferSetupFeeRange AS SELECT 1')
GO

ALTER PROCEDURE AV_GetOfferSetupFeeRange
@Amount INT
AS
BEGIN
	SELECT LoanSizeName, MinSetupFee, MaxSetupFee 
	FROM OfferSetupFeeRanges 
	WHERE @Amount >= FromLoanAmount AND @Amount <= ToLoanAmount
END
GO