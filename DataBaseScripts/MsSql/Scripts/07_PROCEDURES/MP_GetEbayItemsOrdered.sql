IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_GetEbayItemsOrdered]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[MP_GetEbayItemsOrdered]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE MP_GetEbayItemsOrdered
	@iCustomerMarketPlaceId int,
	@iMonthsShiftNumber int
AS
BEGIN
	
	DECLARE @iCountItems int;
	--
	select @iCountItems = SUM(t.QuantityPurchased)
	from MP_EbayOrderItem i
	left join MP_EbayOrder o on i.OrderId = o.Id
	left join MP_EbayTransaction t on t.OrderItemId = i.Id
	where (i.OrderStatus LIKE 'Completed' 
	  OR i.OrderStatus LIKE 'Authenticated'
	  OR i.OrderStatus LIKE 'Shipped')
	   AND o.CustomerMarketPlaceId = @iCustomerMarketPlaceId
	   AND DATEADD(MONTH, @iMonthsShiftNumber, i.CreatedTime ) >= GETDATE()
	                 
	return @iCountItems ;

END
GO
