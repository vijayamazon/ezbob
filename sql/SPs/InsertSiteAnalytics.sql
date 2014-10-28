IF OBJECT_ID('InsertSiteAnalytics') IS NULL
	EXECUTE('CREATE PROCEDURE InsertSiteAnalytics AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE InsertSiteAnalytics
@Date DATETIME,
@CodeName NVARCHAR(300),
@Value INT,
@Source NVARCHAR(300) = NULL
AS
BEGIN
	SET NOCOUNT ON;

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
