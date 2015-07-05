SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('AV_GetOfferSetupFeeRange') IS NULL
	EXECUTE('CREATE PROCEDURE AV_GetOfferSetupFeeRange AS SELECT 1')
GO

ALTER PROCEDURE AV_GetOfferSetupFeeRange
@Amount DECIMAL(18, 6),
@IsNewLoan BIT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1
		r.LoanSizeName,
		r.MinSetupFee,
		r.MaxSetupFee
	FROM
		OfferSetupFeeRanges r
	WHERE
		(
			(r.MaxLoanAmount IS NOT NULL AND @Amount <= r.MaxLoanAmount)
			OR
			r.MaxLoanAmount IS NULL
		)
		AND
		@IsNewLoan = r.IsNewLoan
	ORDER BY
		CASE WHEN r.MaxLoanAmount IS NULL THEN 1 ELSE 0 END,
		r.MaxLoanAmount
END
GO
