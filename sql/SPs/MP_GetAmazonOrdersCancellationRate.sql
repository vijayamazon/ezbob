IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_GetAmazonOrdersCancellationRate]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[MP_GetAmazonOrdersCancellationRate]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[MP_GetAmazonOrdersCancellationRate] 
	(@iCustomerMarketPlaceId int,
	@iMonthsShiftNumber int,
	@CancellationRate float output)
AS
BEGIN
	DECLARE @iCanceledItems float, @iTotalItems float;
	--
	select @iCanceledItems = COUNT(i.Id)
	from MP_AmazonOrderItem2 i
	left join MP_AmazonOrder o on i.AmazonOrderId = o.Id
	where i.OrderStatus LIKE 'Canceled'
		AND o.CustomerMarketPlaceId = @iCustomerMarketPlaceId
		
		AND DATEADD(MONTH, @iMonthsShiftNumber, i.PurchaseDate ) >= GETDATE()

	select @iTotalItems = SUM(i.NumberOfItemsShipped)
	from MP_AmazonOrderItem2 i
	left join MP_AmazonOrder o on i.AmazonOrderId = o.Id
	where (i.OrderStatus LIKE 'Shipped'
		OR i.OrderStatus LIKE 'Canceled')
		AND o.CustomerMarketPlaceId = @iCustomerMarketPlaceId
		
	    AND DATEADD(MONTH, @iMonthsShiftNumber, i.PurchaseDate ) >= GETDATE()
                 
	select @CancellationRate = @iCanceledItems/@iTotalItems
END
GO
