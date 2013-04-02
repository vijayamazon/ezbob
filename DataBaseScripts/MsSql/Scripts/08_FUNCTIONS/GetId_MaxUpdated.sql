IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetId_MaxUpdated]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetId_MaxUpdated]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetId_MaxUpdated] 
(	
	-- Add the parameters for the function here
	@CustomerId int
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
		--and EliminationPassed = 1
		)
	and UpdatingStart is not null
	and UpdatingEnd is not null
	and ERROR is null

	group by CustomerMarketPlaceId

) maxHistory on h.CustomerMarketPlaceId = maxHistory.CustomerMarketPlaceId 
and h.UpdatingStart = maxHistory.MaxUpdatedDate
)
GO
