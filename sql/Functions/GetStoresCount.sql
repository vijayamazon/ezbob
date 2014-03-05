IF OBJECT_ID (N'dbo.GetStoresCount') IS NOT NULL
	DROP FUNCTION dbo.GetStoresCount
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

