IF OBJECT_ID('InsertOrUpdateSiteAnalytics') IS NOT NULL
	DROP PROCEDURE InsertOrUpdateSiteAnalytics
GO

-------------------------------------------------------------------------------

IF TYPE_ID('SiteAnalyticsList') IS NOT NULL
	DROP TYPE SiteAnalyticsList
GO

-------------------------------------------------------------------------------

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-------------------------------------------------------------------------------

CREATE TYPE SiteAnalyticsList AS TABLE (
	[Date] DATETIME NOT NULL,
	CodeName NVARCHAR(300) NOT NULL,
	Value INT NOT NULL,
	Source NVARCHAR(MAX) NULL
)
GO

-------------------------------------------------------------------------------

CREATE PROCEDURE InsertOrUpdateSiteAnalytics
@lst SiteAnalyticsList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	INSERT INTO SiteAnalyticsCodes (Name, Description)
	SELECT DISTINCT
		CodeName,
		'@' + CodeName
	FROM
		@lst l
		LEFT JOIN SiteAnalyticsCodes c ON l.CodeName = c.Name
	WHERE
		c.Id IS NULL

	------------------------------------------------------------------------------

	UPDATE SiteAnalytics SET
		SiteAnalyticsValue = l.Value
	FROM
		@lst l
		INNER JOIN SiteAnalyticsCodes c ON l.CodeName = c.Name
		INNER JOIN SiteAnalytics a
			ON l.[Date] = a.[Date]
			AND c.Id = a.SiteAnalyticsCode
			AND (
				(l.Source IS NULL AND a.Source IS NULL)
				OR
				l.Source = a.Source
			)

	------------------------------------------------------------------------------

	INSERT INTO SiteAnalytics ([Date], SiteAnalyticsCode, SiteAnalyticsValue, Source)
	SELECT
		l.[Date],
		c.Id,
		l.Value,
		l.Source
	FROM
		@lst l
		INNER JOIN SiteAnalyticsCodes c ON l.CodeName = c.Name
		LEFT JOIN SiteAnalytics a
			ON l.[Date] = a.[Date]
			AND c.Id = a.SiteAnalyticsCode
			AND (
				(l.Source IS NULL AND a.Source IS NULL)
				OR
				l.Source = a.Source
			)
	WHERE
		a.Id IS NULL

	------------------------------------------------------------------------------
END
GO
