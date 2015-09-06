SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('dbo.udfGetExperianLtdAnalytics') IS NOT NULL
	DROP FUNCTION dbo.udfGetExperianLtdAnalytics
GO

CREATE FUNCTION dbo.udfGetExperianLtdAnalytics(@ServiceLogID BIGINT, @Now DATETIME)
RETURNS @output TABLE (
	ExperianLtdID BIGINT NULL,
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
		ExperianLtdID, Score, MaxScore, SuggestedAmount, IncorporationDate,
		Sic1980Code1, Sic1980Desc1, Sic1992Code1, Sic1992Desc1,
		AgeOfMostRecentCcj, NumOfCcjsInLast24Months, SumOfCcjsInLast24Months,
		CurrentBalanceSum,
		TangibleEquity,
		AdjustedProfit
	)
	SELECT
		ltd.ExperianLtdID,
		ISNULL(ltd.CommercialDelphiScore, 0),
		ISNULL(ltd.CommercialDelphiScore, 0),
		ISNULL(ltd.CommercialDelphiCreditLimit, 0),
		ltd.IncorporationDate,
		ltd.First1980SICCode,
		ltd.First1980SICCodeDescription,
		ltd.First1992SICCode,
		ltd.First1992SICCodeDescription,
		ISNULL(ltd.AgeOfMostRecentCCJDecreeMonths, 0),
		ISNULL(ltd.NumberOfCCJsDuringLast12Months, 0) + ISNULL(ltd.NumberOfCCJsBetween13And24MonthsAgo, 0),
		ISNULL(ltd.ValueOfCCJsDuringLast12Months, 0) + ISNULL(ltd.ValueOfCCJsBetween13And24MonthsAgo, 0),
		ISNULL((SELECT SUM(dl97.CurrentBalance) FROM ExperianLtdDL97 dl97 WHERE dl97.AccountState = 'A' AND dl97.ExperianLtdID = ltd.ExperianLtdID), 0),
		0,
		0
	FROM
		ExperianLtd ltd
	WHERE
		ltd.ServiceLogID = @ServiceLogID

	------------------------------------------------------------------------------

	IF (SELECT COUNT(*) FROM @output) != 1
		RETURN

	------------------------------------------------------------------------------

	DECLARE @dl99 AS TABLE (
		RowNum BIGINT,
		TangibleEquity DECIMAL(18, 6),
		RetainedEarnings DECIMAL(18, 6),
		TngblAssets DECIMAL(18, 6)
	)

	------------------------------------------------------------------------------

	INSERT INTO @dl99 (RowNum, TangibleEquity, RetainedEarnings, TngblAssets)
	SELECT TOP 2
		ROW_NUMBER() OVER (ORDER BY dl99.[Date] DESC),
		ISNULL(dl99.TotalShareFund, 0) -
		ISNULL(dl99.InTngblAssets, 0) -
		ISNULL(dl99.DebtorsDirLoans, 0) +
		ISNULL(dl99.CredDirLoans, 0) +
		ISNULL(dl99.OnClDirLoans, 0),
		ISNULL(dl99.RetainedEarnings, 0),
		ISNULL(dl99.TngblAssets, 0)
	FROM
		ExperianLtdDL99 dl99
		INNER JOIN @output o ON dl99.ExperianLtdID = o.ExperianLtdID
	WHERE
		dl99.[Date] IS NOT NULL
	ORDER BY
		dl99.[Date]

	------------------------------------------------------------------------------

	IF NOT EXISTS (SELECT * FROM @dl99 WHERE RowNum = 1)
		INSERT INTO @dl99 (RowNum, TangibleEquity, RetainedEarnings, TngblAssets) VALUES (1, 0, 0, 0)

	------------------------------------------------------------------------------

	IF NOT EXISTS (SELECT * FROM @dl99 WHERE RowNum = 2)
		INSERT INTO @dl99 (RowNum, TangibleEquity, RetainedEarnings, TngblAssets) VALUES (2, 0, 0, 0)

	------------------------------------------------------------------------------

	UPDATE @output SET
		TangibleEquity = d.TangibleEquity,
		AdjustedProfit = d.RetainedEarnings
	FROM
		@dl99 d
	WHERE
		d.RowNum = 1

	------------------------------------------------------------------------------

	UPDATE @output SET
		AdjustedProfit = AdjustedProfit - d.RetainedEarnings + d.TngblAssets / 5
	FROM
		@dl99 d
	WHERE
		d.RowNum = 2

	------------------------------------------------------------------------------

	DECLARE @owners AS TABLE (
		RefNum NVARCHAR(255),
		ServiceLogID BIGINT NULL,
		Score INT NULL
	)

	------------------------------------------------------------------------------

	INSERT INTO @owners (RefNum)
	SELECT
		ltd.RegisteredNumberOfTheCurrentUltimateParentCompany
	FROM
		ExperianLtd ltd
	WHERE
		ltd.ServiceLogID = @ServiceLogID
	UNION
	SELECT
		s.RegisteredNumberOfALimitedCompanyWhichIsAShareholder
	FROM
		ExperianLtdShareholders s
		INNER JOIN ExperianLtd ltd ON s.ExperianLtdID = ltd.ExperianLtdID
	WHERE
		ltd.ServiceLogID = @ServiceLogID

	------------------------------------------------------------------------------

	UPDATE @owners SET
		ServiceLogID = dbo.udfGetCompanyHistoricalLogID(RefNum, @Now)

	------------------------------------------------------------------------------

	UPDATE @owners SET
		Score = ISNULL(ltd.CommercialDelphiScore, 0)
	FROM
		@owners o
		INNER JOIN ExperianLtd ltd ON o.ServiceLogID = ltd.ServiceLogID

	------------------------------------------------------------------------------

	UPDATE @owners SET
		Score = 0
	WHERE
		Score IS NULL

	------------------------------------------------------------------------------

	DECLARE @MaxScore INT = ISNULL((SELECT MAX(Score) FROM @owners), 0)

	------------------------------------------------------------------------------

	UPDATE @output SET
		MaxScore = dbo.udfMaxInt(MaxScore, @MaxScore)

	------------------------------------------------------------------------------

	RETURN
END
GO
