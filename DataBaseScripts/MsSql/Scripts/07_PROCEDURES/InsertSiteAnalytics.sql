IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InsertSiteAnalytics]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[InsertSiteAnalytics]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE InsertSiteAnalytics
@Date DATETIME,
@CodeName NVARCHAR(300),
@Value INT
AS
BEGIN	
	IF NOT EXISTS (SELECT Id FROM SiteAnalyticsCodes WHERE Name = @CodeName)
		INSERT INTO SiteAnalyticsCodes (Name, Description)
			Values (@CodeName, '@' + @CodeName)

	INSERT INTO SiteAnalytics ([Date], SiteAnalyticsCode, SiteAnalyticsValue)
	SELECT
		@Date,
		c.Id,
		@Value
	FROM
		SiteAnalyticsCodes c
	WHERE
		c.Name = @CodeName
END
GO
