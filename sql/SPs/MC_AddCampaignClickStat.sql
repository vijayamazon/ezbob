IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MC_AddCampaignClickStat]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[MC_AddCampaignClickStat]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[MC_AddCampaignClickStat] 
	(@Title    	 NVARCHAR(300)
   ,@Url         NVARCHAR(300)
   ,@Email       NVARCHAR(300)
   ,@EmailsSent  INT
   ,@Clicks      INT
   ,@Date        DATETIME)
AS
BEGIN
	INSERT INTO MC_CampaignClicks (Date, Title, Url ,EmailsSent ,Clicks ,Email) VALUES (@Date, @Title, @Url ,@EmailsSent ,@Clicks ,@Email)
END
GO
