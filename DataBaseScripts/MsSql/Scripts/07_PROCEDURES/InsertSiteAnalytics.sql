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

IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID('MC_AddCampaignClickStat') AND OBJECTPROPERTY(id,N'IsProcedure') = 1) 
BEGIN
	PRINT 'MC_AddCampaignClickStat exists, droping'
	DROP PROCEDURE MC_AddCampaignClickStat 
END
GO
