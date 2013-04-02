IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_GeAnnualSalesEBay]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[MP_GeAnnualSalesEBay]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE MP_GeAnnualSalesEBay
	@iCustomerMarketPlaceId int

AS
BEGIN
	
select SUM(TotalTable12.Price * TotalTable12.Quantity / TotalTable12.Rate) 
 from 
  (
    select et8.price as Price,
           et8.QuantityPurchased as Quantity, 
          (select TableRate.Rate from dbo.MP_GetCurrencyRate1(eoi8.AmountPaidCurrency, eoi8.CreatedTime) TableRate) as Rate
   from MP_EbayOrderItem eoi8
   left join MP_EbayOrder eo8 on eoi8.OrderId = eo8.Id
   left join MP_EbayTransaction et8 on et8.OrderItemId = eoi8.Id
   left join MP_CustomerMarketPlace cmp ON eo8.CustomerMarketPlaceId = cmp.Id
   where (eoi8.OrderStatus LIKE 'Completed' 
    OR eoi8.OrderStatus LIKE 'Authenticated'
    OR eoi8.OrderStatus LIKE 'Shipped') and
    cmp.Id= @iCustomerMarketPlaceId
     AND DATEADD(MONTH, 12, eoi8.CreatedTime ) >= GETDATE()
  ) as TotalTable12



END
GO
