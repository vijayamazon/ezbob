IF OBJECT_ID (N'dbo.GetId_MaxUpdated') IS NOT NULL
	DROP FUNCTION dbo.GetId_MaxUpdated
GO

CREATE FUNCTION [dbo].[GetId_MaxUpdated]
(	@CustomerId int
)
RETURNS TABLE 
AS
RETURN 
(
	select  h.CustomerMarketPlaceId, h.UpdatingStart as Updated, h.Id as UpdateHistoryId
from MP_CustomerMarketPlaceUpdatingHistory h 
inner join 
(
	select CustomerMarketPlaceId, MAX(UpdatingStart) as MaxUpdatedDate
	from MP_CustomerMarketPlaceUpdatingHistory
	where CustomerMarketPlaceId in 
		(
		select id 
		from MP_CustomerMarketPlace
		where CustomerId = @CustomerId
		)
	and UpdatingStart is not null
	and UpdatingEnd is not null
	and ERROR is null

	group by CustomerMarketPlaceId

) maxHistory on h.CustomerMarketPlaceId = maxHistory.CustomerMarketPlaceId 
and h.UpdatingStart = maxHistory.MaxUpdatedDate
)

GO

