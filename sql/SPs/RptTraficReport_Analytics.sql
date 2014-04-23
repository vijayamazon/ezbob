IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptTraficReport_Analytics]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptTraficReport_Analytics]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[RptTraficReport_Analytics] 
	(@DateStart DATE, @DateEnd DATE)
AS
BEGIN

SELECT s1.Source, sum(s1.SiteAnalyticsValue) AS 'Visits', sum(s2.SiteAnalyticsValue) AS 'Visitors'
FROM SiteAnalytics s1 
INNER JOIN SiteAnalytics s2 ON s1.[Date]=s2.[Date] AND s1.Source=s2.Source AND s1.SiteAnalyticsCode<s2.SiteAnalyticsCode 
INNER JOIN SiteAnalyticsCodes c1 ON c1.Id = s1.SiteAnalyticsCode  
INNER JOIN SiteAnalyticsCodes c2 ON c2.Id = s2.SiteAnalyticsCode  
WHERE s1.[Date]>=@DateStart 
AND s1.[Date]<@DateEnd 
GROUP BY s1.Source, c1.Name, c2.Name

END