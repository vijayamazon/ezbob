IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetMarketplaceType]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetMarketplaceType]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetMarketplaceType]
	(@MarketplaceId INT) 
AS
BEGIN
	SELECT 
		Name
	FROM
		MP_MarketplaceType,
		MP_CustomerMarketPlace
	WHERE
		MP_MarketplaceType.Id = MP_CustomerMarketPlace.MarketPlaceId AND
		MP_CustomerMarketPlace.Id = @MarketplaceId
END
GO
