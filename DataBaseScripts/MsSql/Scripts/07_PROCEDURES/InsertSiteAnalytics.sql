IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InsertSiteAnalytics]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[InsertSiteAnalytics]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE InsertSiteAnalytics
	 @Date		             DATETIME
	,@CodeName              NVARCHAR(300)
	,@Value				 INT
AS
BEGIN	
	DECLARE @CodeId INT = (SELECT Id FROM SiteAnalyticsCodes WHERE Name=@CodeName)
	INSERT INTO SiteAnalytics ([Date],SiteAnalyticsCode,SiteAnalyticsValue) VALUES(@Date, @CodeId, @Value)
END
GO
