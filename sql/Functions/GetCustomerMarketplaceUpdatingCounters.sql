IF OBJECT_ID (N'dbo.GetCustomerMarketplaceUpdatingCounters') IS NOT NULL
	DROP FUNCTION dbo.GetCustomerMarketplaceUpdatingCounters
GO

CREATE FUNCTION [dbo].[GetCustomerMarketplaceUpdatingCounters]
(	@marketplaceId int
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT c.Method, COUNT(*) AS Counter
  FROM MP_CustomerMarketplaceUpdatingCounter c
  left join MP_CustomerMarketplaceUpdatingActionLog l on l.Id = c.CustomerMarketplaceUpdatingActionLogId
  left join MP_CustomerMarketPlaceUpdatingHistory h on l.CustomerMarketplaceUpdatingHistoryRecordId = h.Id
  where h.CustomerMarketPlaceId = @marketplaceId--149
  group by c.method	
)

GO

