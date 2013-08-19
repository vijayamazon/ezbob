IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_GetAmazonOrdersNumber]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[MP_GetAmazonOrdersNumber]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE MP_GetAmazonOrdersNumber
	@iCustomerMarketPlaceId int,
	@iMonthsShiftNumber int
AS
BEGIN

DECLARE @iOrdersNumber int;
--	
select @iOrdersNumber = COUNT(i.Id)
from MP_AmazonOrderItem2 i
left join MP_AmazonOrder o on i.AmazonOrderId = o.Id
where i.OrderStatus LIKE 'Shipped'
   AND o.CustomerMarketPlaceId = @iCustomerMarketPlaceId

   AND DATEADD(MONTH, @iMonthsShiftNumber, i.PurchaseDate ) >= GETDATE()

return @iOrdersNumber;
	
END
GO
