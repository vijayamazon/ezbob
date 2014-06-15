IF OBJECT_ID('QuickOfferLoadConfiguration') IS NULL
	EXECUTE('CREATE PROCEDURE QuickOfferLoadConfiguration AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE QuickOfferLoadConfiguration
AS
BEGIN
	SET NOCOUNT ON;

	IF NOT EXISTS (SELECT * FROM QuickOfferConfiguration WHERE ID = 1)
		INSERT INTO QuickOfferConfiguration (ID) VALUES (1)

	SELECT
		Enabled,
		CompanySeniorityMonths,
		ApplicantMinAgeYears,
		NoDefaultsInLastMonths,
		AmlMin,
		PersonalScoreMin,
		BusinessScoreMin,
		MaxLoanCountPerDay,
		MaxIssuedValuePerDay,
		OfferDurationHours,
		MinOfferAmount,
		OfferCapPct,
		ImmediateMaxAmount,
		ImmediateTermMonths,
		ImmediateInterestRate,
		ImmediateSetupFee,
		PotentialMaxAmount,
		PotentialTermMonths,
		PotentialSetupFee,
		OfferAmountCalculator,
		PriceCalculator
	FROM
		QuickOfferConfiguration
	WHERE
		ID = 1
END
GO
