IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetCustomerMarketplaceUpdatingCounters]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetCustomerMarketplaceUpdatingCounters]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
2012-11-01 O.Zemskyi Customer Marketplace Updating Counters
*/
CREATE FUNCTION [dbo].[GetCustomerMarketplaceUpdatingCounters]
(	
  @marketplaceId int
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
