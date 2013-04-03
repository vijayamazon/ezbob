IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetStoresCount]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetStoresCount]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetStoresCount]
(	
	@storeType int,
	@dateStart DateTime, 
	@dateEnd DateTime
)
RETURNS TABLE 
AS
RETURN 
(
	Select Count(MP_CustomerMarketPlace.Id) StoresCount, CustomerId FROM
	(SELECT Id
		FROM dbo.MP_CustomerMarketPlace
			Where MarketPlaceId = @storeType
	) as m LEFT OUTER JOIN 
	(SELECT CustomerMarketPlaceId
		FROM dbo.MP_CustomerMarketPlaceUpdatingHistory
			Where UpdatingEnd <= @dateEnd 
			AND UpdatingStart >= @dateStart
		GROUP BY CustomerMarketPlaceId
	) as um ON um.CustomerMarketPlaceId = m.Id
	LEFT OUTER JOIN MP_CustomerMarketPlace ON
	MP_CustomerMarketPlace.Id = m.Id
	Group By CustomerId
)
GO
