IF OBJECT_ID('CustomerAnalytics') IS NULL
	EXECUTE('CREATE VIEW CustomerAnalytics AS SELECT 1 AS FieldName')
GO

ALTER VIEW CustomerAnalytics AS
SELECT
	ISNULL(
		ISNULL(
			ISNULL(
				p.CustomerID,
				c.CustomerID
			),
			d.CustomerID
		),
		l.CustomerID
	)                               AS CustomerID,
	dbo.udfMaxDate(
		dbo.udfMaxDate(
			dbo.udfMaxDate(
				p.AnalyticsDate,
				c.AnalyticsDate
			),
			d.AnalyticsDate
		),
		l.AnalyticsDate
	)                               AS AnalyticsDate,
	ISNULL(p.Score, 0)              AS PersonalScore,
	ISNULL(d.MinScore, 0)           AS PersonalMinScore,
	ISNULL(d.MaxScore, 0)           AS PersonalMaxScore,
	ISNULL(p.IndebtednessIndex, 0)  AS IndebtednessIndex,
	CASE ISNULL(p.NumOfAccounts, 0)
		WHEN 0 THEN 1
		ELSE 0
	END                             AS ThinFile,
	ISNULL(p.NumOfAccounts, 0)      AS NumOfAccounts,
	ISNULL(p.NumOfDefaults, 0)      AS NumOfDefaults,
	ISNULL(p.NumOfLastDefaults, 0)  AS NumOfLastDefaults,
	ISNULL(c.Score, 0)              AS CompanyScore,
	ISNULL(c.SuggestedAmount, 0)    AS SuggestedAmount,
	ISNULL(l.AnnualTurnover, 0)     AS AnnualTurnover,
	c.IncorporationDate             AS IncorporationDate
FROM (
		SELECT
			CustomerID,
			AnalyticsDate,
			Score,
			IndebtednessIndex,
			NumOfAccounts,
			NumOfDefaults,
			NumOfLastDefaults
		FROM
			CustomerAnalyticsPersonal
		WHERE
			IsActive = 1
	) p FULL OUTER JOIN (
		SELECT
			CustomerID,
			AnalyticsDate,
			Score,
			SuggestedAmount,
			IncorporationDate
		FROM
			CustomerAnalyticsCompany
		WHERE
			IsActive = 1
	) c ON p.CustomerID = c.CustomerID
	FULL OUTER JOIN (
		SELECT
			CustomerID,
			AnalyticsDate,
			MinScore,
			MaxScore
		FROM
			CustomerAnalyticsDirector
		WHERE
			IsActive = 1
	) d ON ISNULL(p.CustomerID, c.CustomerID) = d.CustomerID
	FULL OUTER JOIN (
		SELECT
			CustomerID,
			AnalyticsDate,
			AnnualTurnover
		FROM
			CustomerAnalyticsLocalData
		WHERE
			IsActive = 1
	) l ON ISNULL(p.CustomerID, ISNULL(c.CustomerID, d.CustomerID)) = l.CustomerID
GO
