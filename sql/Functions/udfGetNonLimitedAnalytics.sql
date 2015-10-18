SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('dbo.udfGetNonLimitedAnalytics') IS NOT NULL
	DROP FUNCTION dbo.udfGetNonLimitedAnalytics
GO

-------------------------------------------------------------------------------
--
-- Output table MUST be in the same format in all these functions:
-- udfGetLimitedAnalytics
-- udfGetNonLimitedAnalytics
-- udfGetCustomerCompanyAnalytics
--
-------------------------------------------------------------------------------

CREATE FUNCTION dbo.udfGetNonLimitedAnalytics(@ServiceLogID BIGINT)
RETURNS @output TABLE (
	ServiceLogID BIGINT NULL,
	ParsedRootEntryID BIGINT NULL,
	Score INT NULL,
	MaxScore INT NULL,
	SuggestedAmount DECIMAL(18, 6) NULL,
	IncorporationDate DATETIME NULL,
	CurrentBalanceSum INT NULL,
	TangibleEquity DECIMAL(18, 6) NULL,
	AdjustedProfit DECIMAL(18, 6) NULL,
	Sic1980Code1 NVARCHAR(4) NULL,
	Sic1980Desc1 NVARCHAR(75) NULL,
	Sic1992Code1 NVARCHAR(4) NULL,
	Sic1992Desc1 NVARCHAR(75) NULL,
	AgeOfMostRecentCcj INT NULL,
	NumOfCcjsInLast24Months INT NULL,
	SumOfCcjsInLast24Months INT NULL
)
AS
BEGIN
	INSERT INTO @output (
		ServiceLogID, ParsedRootEntryID,
		Score, MaxScore, SuggestedAmount, IncorporationDate,
		Sic1980Code1, Sic1980Desc1, Sic1992Code1, Sic1992Desc1,
		AgeOfMostRecentCcj, NumOfCcjsInLast24Months, SumOfCcjsInLast24Months,
		CurrentBalanceSum,
		TangibleEquity,
		AdjustedProfit
	)
	SELECT
		@ServiceLogID,
		nltd.Id,
		ISNULL(nltd.RiskScore, 0),
		ISNULL(nltd.RiskScore, 0),
		ISNULL(nltd.CreditLimit, 0),
		nltd.IncorporationDate,
		'',
		'',
		'',
		'',
		ISNULL(nltd.AgeOfMostRecentJudgmentDuringOwnershipMonths, 0),
		ISNULL(nltd.TotalJudgmentCountLast24Months, 0) + ISNULL(nltd.TotalAssociatedJudgmentCountLast24Months, 0),
		ISNULL(nltd.TotalJudgmentValueLast24Months, 0) + ISNULL(nltd.TotalAssociatedJudgmentValueLast24Months, 0),
		0,
		0,
		0
	FROM
		ExperianNonLimitedResults nltd
	WHERE
		nltd.ServiceLogID = @ServiceLogID
		AND
		nltd.IsActive = 1

	------------------------------------------------------------------------------

	RETURN
END
GO
