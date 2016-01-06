SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('UC_CampaignSourceRef') IS NULL
	ALTER TABLE CampaignSourceRef ADD CONSTRAINT UC_CampaignSourceRef UNIQUE (CustomerId)
GO
