﻿IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_GetAmazonOrdersCancelled]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[MP_GetAmazonOrdersCancelled]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE MP_GetAmazonOrdersCancelled
	@iCustomerMarketPlaceId int,
	@iMonthsShiftNumber int
AS
BEGIN
	
DECLARE @iOrdersNumber int;
--	
select @iOrdersNumber = Count(i.Id)
from MP_AmazonOrderItem2 i
left join MP_AmazonOrder o on i.AmazonOrderId = o.Id
where i.OrderStatus LIKE 'Canceled'
   AND o.CustomerMarketPlaceId = @iCustomerMarketPlaceId
   AND i.MarketplaceId LIKE 'A1F83G8C2ARO7P'
   AND DATEADD(MONTH, @iMonthsShiftNumber, i.PurchaseDate ) >= GETDATE()
	                 
	return @iOrdersNumber;

END
GO
