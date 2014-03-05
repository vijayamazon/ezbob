IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_GetEbayOrdersCancellationRate]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[MP_GetEbayOrdersCancellationRate]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[MP_GetEbayOrdersCancellationRate] 
	(@iCustomerMarketPlaceId int,
	@iMonthsShiftNumber int,
	@CancellationRate float output)
AS
BEGIN
	DECLARE @iCanceledItems float, @iTotalItems float;
	--
	select @iCanceledItems = COUNT(i.Id)
	from MP_EbayOrderItem i
	left join MP_EbayOrder o on i.OrderId = o.Id
	where i.OrderStatus LIKE 'Cancelled' 
	   AND o.CustomerMarketPlaceId = @iCustomerMarketPlaceId
	   AND DATEADD(MONTH, @iMonthsShiftNumber, i.CreatedTime ) >= GETDATE()

	select @iTotalItems = SUM(t.QuantityPurchased)
	from MP_EbayOrderItem i
	left join MP_EbayOrder o on i.OrderId = o.Id
	left join MP_EbayTransaction t on t.OrderItemId = i.Id
	where (i.OrderStatus LIKE 'Completed' 
	  OR i.OrderStatus LIKE 'Authenticated'
	  OR i.OrderStatus LIKE 'Shipped'
	  OR i.OrderStatus LIKE 'Cancelled')
	   AND o.CustomerMarketPlaceId = @iCustomerMarketPlaceId
	   AND DATEADD(MONTH, @iMonthsShiftNumber, i.CreatedTime ) >= GETDATE()
                 
	select @CancellationRate = @iCanceledItems/@iTotalItems
END
GO
