IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[QuickOfferLoadConfiguration]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[QuickOfferLoadConfiguration]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[QuickOfferLoadConfiguration]
AS
BEGIN
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
