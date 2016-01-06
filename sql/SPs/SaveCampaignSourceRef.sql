SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCampaignSourceRef') IS NOT NULL
	DROP PROCEDURE SaveCampaignSourceRef
GO

IF TYPE_ID('CampaignSourceRefList') IS NOT NULL
	DROP TYPE CampaignSourceRefList
GO
