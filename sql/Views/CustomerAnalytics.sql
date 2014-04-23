IF OBJECT_ID('CustomerAnalytics') IS NULL
	EXECUTE('CREATE VIEW CustomerAnalytics AS SELECT 1 AS FieldName')
GO

ALTER VIEW CustomerAnalytics AS
SELECT
	p.CustomerID                    AS CustomerID,
	dbo.udfMaxDate(                 -- AnalyticsDate
		p.AnalyticsDate,           -- AnalyticsDate
		c.AnalyticsDate            -- AnalyticsDate
	)                               AS AnalyticsDate,
	ISNULL(p.Score, 0)              AS PersonalScore,
	ISNULL(p.MinScore, 0)           AS PersonalMinScore,
	ISNULL(p.MaxScore, 0)           AS PersonalMaxScore,
	ISNULL(p.IndebtednessIndex, 0)  AS IndebtednessIndex,
	CASE ISNULL(p.NumOfAccounts, 0) -- ThinFile
		WHEN 0 THEN 1              -- ThinFile
		ELSE 0                     -- ThinFile
	END                             AS ThinFile,
	ISNULL(p.NumOfAccounts, 0)      AS NumOfAccounts,
	ISNULL(p.NumOfDefaults, 0)      AS NumOfDefaults,
	ISNULL(p.NumOfLastDefaults, 0)  AS NumOfLastDefaults,
	ISNULL(c.Score, 0)              AS CompanyScore,
	ISNULL(c.SuggestedAmount, 0)    AS SuggestedAmount,
	ISNULL(c.AnnualTurnover, 0)     AS AnnualTurnover,
	c.IncorporationDate             AS IncorporationDate
FROM
	CustomerAnalyticsPersonal p
	LEFT JOIN CustomerAnalyticsCompany c
		ON p.CustomerID = c.CustomerID
		AND p.IsActive = 1
		AND c.IsActive = 1
UNION
SELECT
	c.CustomerID                    AS CustomerID,
	dbo.udfMaxDate(                 -- AnalyticsDate
		p.AnalyticsDate,           -- AnalyticsDate
		c.AnalyticsDate            -- AnalyticsDate
	)                               AS AnalyticsDate,
	ISNULL(p.Score, 0)              AS PersonalScore,
	ISNULL(p.MinScore, 0)           AS PersonalMinScore,
	ISNULL(p.MaxScore, 0)           AS PersonalMaxScore,
	ISNULL(p.IndebtednessIndex, 0)  AS IndebtednessIndex,
	CASE ISNULL(p.NumOfAccounts, 0) -- ThinFile
		WHEN 0 THEN 1              -- ThinFile
		ELSE 0                     -- ThinFile
	END                             AS ThinFile,
	ISNULL(p.NumOfAccounts, 0)      AS NumOfAccounts,
	ISNULL(p.NumOfDefaults, 0)      AS NumOfDefaults,
	ISNULL(p.NumOfLastDefaults, 0)  AS NumOfLastDefaults,
	ISNULL(c.Score, 0)              AS CompanyScore,
	ISNULL(c.SuggestedAmount, 0)    AS SuggestedAmount,
	ISNULL(c.AnnualTurnover, 0)     AS AnnualTurnover,
	c.IncorporationDate             AS IncorporationDate
FROM
	CustomerAnalyticsPersonal p
	RIGHT JOIN CustomerAnalyticsCompany c
		ON p.CustomerID = c.CustomerID
		AND p.IsActive = 1
		AND c.IsActive = 1
GO
