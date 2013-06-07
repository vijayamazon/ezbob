IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptSiteAnalytics]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptSiteAnalytics]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE RptSiteAnalytics
	@DateStart    DATETIME,
	@DateEnd      DATETIME
AS
BEGIN
 
SELECT sa.[Date],sac.Name, sac.Description, sa.SiteAnalyticsValue FROM SiteAnalytics sa, SiteAnalyticsCodes sac WHERE sa.SiteAnalyticsCode = sac.Id AND sa.[Date] BETWEEN @DateStart AND @DateEnd

END
GO
