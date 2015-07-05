IF OBJECT_ID('LoadOfferRanges') IS NULL
	EXECUTE('CREATE PROCEDURE LoadOfferRanges AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadOfferRanges
@Amount DECIMAL(18, 6),
@IsNewLoan BIT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @Name NVARCHAR(50)
	DECLARE @MinSetupFee DECIMAL(18, 6)
	DECLARE @MaxSetupFee DECIMAL(18, 6)

	SELECT TOP 1
		@Name = r.LoanSizeName,
		@MinSetupFee = r.MinSetupFee,
		@MaxSetupFee = r.MaxSetupFee
	FROM
		OfferSetupFeeRanges r
	WHERE
		r.MaxLoanAmount IS NOT NULL
		AND
		@Amount <= r.MaxLoanAmount
		AND
		@IsNewLoan = r.IsNewLoan
	ORDER BY
		r.MaxLoanAmount

	IF @MinSetupFee IS NULL
	BEGIN
		SELECT TOP 1
			@Name = r.LoanSizeName,
			@MinSetupFee = r.MinSetupFee,
			@MaxSetupFee = r.MaxSetupFee
		FROM
			OfferSetupFeeRanges r
		WHERE
			r.MaxLoanAmount IS NULL
			AND
			@IsNewLoan = r.IsNewLoan
	END

	SELECT
		@Name AS LoanSizeName,
		@MinSetupFee AS MinSetupFee,
		@MaxSetupFee AS MaxSetupFee
END
GO
