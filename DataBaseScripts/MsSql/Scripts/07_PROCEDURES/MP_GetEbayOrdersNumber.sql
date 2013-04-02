IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_GetEbayOrdersNumber]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[MP_GetEbayOrdersNumber]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE MP_GetEbayOrdersNumber
	@iCustomerMarketPlaceId int,
	@iMonthsShiftNumber int
AS
BEGIN

DECLARE @iOrdersNumber int;
--	
select @iOrdersNumber = COUNT(o.Id)
from MP_EbayOrderItem i
left join MP_EbayOrder o on i.OrderId = o.Id
where (i.OrderStatus LIKE 'Completed' 
   OR i.OrderStatus LIKE 'Authenticated'
   OR i.OrderStatus LIKE 'Shipped')
   AND o.CustomerMarketPlaceId = @iCustomerMarketPlaceId
   AND DATEADD(MONTH, @iMonthsShiftNumber, i.CreatedTime ) >= GETDATE()

return @iOrdersNumber;
	
END
GO
