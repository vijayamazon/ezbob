IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptGetCampaignClickStats]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptGetCampaignClickStats]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE RptGetCampaignClickStats
	@DateStart    DATETIME
   ,@DateEnd      DATETIME
AS
BEGIN
	SELECT [Date], Title, Url, EmailsSent, Clicks, Email FROM MC_CampaignClicks ORDER BY [Date] DESC, Title
	--SELECT * FROM
	--	(SELECT Title,'' Url, EmailsSent, Clicks, Date , '' AS Email FROM MC_CampaignClicks GROUP BY Title, EmailsSent, Clicks, Date
	--	UNION 
	--	SELECT Title, Url, '' EmailsSent, '' Clicks,'' Date , Email FROM MC_CampaignClicks) x
	--ORDER BY x.Title, x.Date
END
GO
