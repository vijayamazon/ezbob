IF NOT EXISTS (SELECT * from sys.columns WHERE Name = N'Source' and Object_ID = Object_ID(N'SiteAnalytics'))
BEGIN 
	ALTER TABLE SiteAnalytics ADD Source NVARCHAR(300)
	INSERT INTO SiteAnalyticsCodes (Name, Description) VALUES ('SourceVisitors', 'Site visitors from source')
	INSERT INTO SiteAnalyticsCodes (Name, Description) VALUES ('SourceVisits', 'Site new visits from source')
END 	

GO 

ALTER PROCEDURE InsertSiteAnalytics
@Date DATETIME,
@CodeName NVARCHAR(300),
@Value INT,
@Source NVARCHAR(300) = NULL
AS
BEGIN	
	IF NOT EXISTS (SELECT Id FROM SiteAnalyticsCodes WHERE Name = @CodeName)
		INSERT INTO SiteAnalyticsCodes (Name, Description)
			Values (@CodeName, '@' + @CodeName)

	INSERT INTO SiteAnalytics ([Date], SiteAnalyticsCode, SiteAnalyticsValue, Source)
	SELECT
		@Date,
		c.Id,
		@Value,
		@Source
	FROM
		SiteAnalyticsCodes c
	WHERE
		c.Name = @CodeName
END

GO

ALTER PROCEDURE RptSiteAnalytics
@DateStart DATETIME,
@DateEnd   DATETIME
AS
BEGIN
	SELECT
		sa.[Date],
		sac.Name,
		sac.Description,
		sa.SiteAnalyticsValue,
		sa.Source
	FROM
		SiteAnalytics sa
		INNER JOIN SiteAnalyticsCodes sac ON sa.SiteAnalyticsCode = sac.Id
	WHERE
		CONVERT(DATE, @DateStart) <= sa.[Date] AND sa.[Date] < CONVERT(DATE, @DateEnd)
END

UPDATE ReportScheduler SET Header='Date,Analytics Name,Analytics Description,Analytics Value, Source' WHERE Type='RPT_SITE_ANALYTICS'
UPDATE ReportScheduler SET Fields='Date,Name,Description,SiteAnalyticsValue,Source' WHERE Type='RPT_SITE_ANALYTICS'
GO

