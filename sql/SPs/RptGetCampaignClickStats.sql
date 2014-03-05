IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptGetCampaignClickStats]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptGetCampaignClickStats]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptGetCampaignClickStats] 
	(@DateStart DATETIME,
@DateEnd   DATETIME)
AS
BEGIN
	SELECT
		[Date],
		Title,
		Url,
		EmailsSent,
		Clicks,
		Email
	FROM
		MC_CampaignClicks
	ORDER BY
		[Date] DESC,
		Title
END
GO
