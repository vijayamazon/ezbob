IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_GetAmazonOrdersTotalSum]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[MP_GetAmazonOrdersTotalSum]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[MP_GetAmazonOrdersTotalSum] 
	(@iCustomerMarketPlaceId int,
	@iMonthsShiftNumber int)
AS
BEGIN
	DECLARE @iPaidAmount int;
 --
 select @iPaidAmount = SUM(i.OrderTotal*i.NumberOfItemsShipped* dbo.MP_GetCurrencyRate(i.OrderTotalCurrency, i.PurchaseDate) )
 from MP_AmazonOrderItem2 i
 left join MP_AmazonOrder o on i.AmazonOrderId = o.Id
where i.OrderStatus LIKE 'Shipped'
   AND o.CustomerMarketPlaceId = @iCustomerMarketPlaceId
   AND DATEADD(MONTH, @iMonthsShiftNumber, i.PurchaseDate ) >= GETDATE()
                  
 return @iPaidAmount
END
GO
