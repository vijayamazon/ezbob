IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetMarketplaceDetailsForUpdate]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetMarketplaceDetailsForUpdate]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetMarketplaceDetailsForUpdate] 
	(@MarketplaceId INT)
AS
BEGIN
	DECLARE @notUpdatedCount INT
	SELECT 
		MP_MarketplaceType.Name,
		MP_CustomerMarketPlace.Disabled,
		MP_CustomerMarketPlace.DisplayName
	FROM 
		MP_MarketplaceType, 
		MP_CustomerMarketPlace				
	WHERE 
		MP_CustomerMarketPlace.Id = @MarketplaceId AND
		MP_MarketplaceType.Id = MP_CustomerMarketPlace.MarketPlaceId
END
GO
