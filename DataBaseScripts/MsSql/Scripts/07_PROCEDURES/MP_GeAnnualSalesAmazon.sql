IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_GeAnnualSalesAmazon]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[MP_GeAnnualSalesAmazon]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE MP_GeAnnualSalesAmazon
		@iCustomerMarketPlaceId int
AS
BEGIN


select SUM(TotalTable3.Price * TotalTable3.Quantity / TotalTable3.Rate) 
 from 
   (
    select aoi8.OrderTotal as Price,
           aoi8.NumberOfItemsShipped as Quantity, 
          (select TableRate.Rate from dbo.MP_GetCurrencyRate1(aoi8.OrderTotalCurrency, aoi8.PurchaseDate) TableRate) as Rate
     from  MP_AmazonOrder ao8
    left join MP_AmazonOrderItem2 aoi8 on aoi8.AmazonOrderId = ao8.Id 
    left join MP_CustomerMarketPlace cmp1 ON  ao8.CustomerMarketPlaceId = cmp1.Id
    where aoi8.OrderStatus LIKE 'Shipped' and
		cmp1.Id= @iCustomerMarketPlaceId

          AND DATEADD(MONTH, 12, aoi8.PurchaseDate ) >= GETDATE()
  )as TotalTable3


END
GO
