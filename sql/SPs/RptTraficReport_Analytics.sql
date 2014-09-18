IF OBJECT_ID('RptTraficReport_Analytics') IS NULL
	EXECUTE('CREATE PROCEDURE RptTraficReport_Analytics AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptTraficReport_Analytics
@DateStart DATE,
@DateEnd DATE
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		s1.Source,
		SUM(s1.SiteAnalyticsValue) AS 'Visits',
		SUM(s2.SiteAnalyticsValue) AS 'Visitors'
	FROM
		SiteAnalytics s1 
		INNER JOIN SiteAnalytics s2
			ON s1.[Date] = s2.[Date]
			AND s1.Source = s2.Source
			AND s1.SiteAnalyticsCode < s2.SiteAnalyticsCode 
		INNER JOIN SiteAnalyticsCodes c1 ON c1.Id = s1.SiteAnalyticsCode  
		INNER JOIN SiteAnalyticsCodes c2 ON c2.Id = s2.SiteAnalyticsCode  
	WHERE
		@DateStart <= s1.[Date] AND s1.[Date] < @DateEnd 
	GROUP BY
		s1.Source,
		c1.Name,
		c2.Name
END
GO
