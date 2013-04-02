IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_GetInventoryCashValue]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[MP_GetInventoryCashValue]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[MP_GetInventoryCashValue]
	@iCustomerMarketPlaceId int,
	@iMarketPlaceId int
AS
BEGIN
select    
(select SUM(TableInventoryCashValue.Quantity * TableInventoryCashValue.Price / TableInventoryCashValue.Rate)
  from
  (
   select ii1.Quantity as Quantity,
       ii1.Price as Price,
       (select TableRate.Rate from dbo.MP_GetCurrencyRate1(ii1.Currency, NULL) TableRate) as Rate
       --dbo.MP_GetCurrencyRate(ii.Currency, NULL ))
             
   from MP_EbayAmazonInventory inv1 
   LEFT JOIN MP_EbayAmazonInventoryItem ii1 on ii1.InventoryId = inv1.Id
   where inv1.CustomerMarketPlaceId = cmp.Id  
   and inv1.Created = (select MAX(inv2.Created)
      from MP_EbayAmazonInventory inv2
      where inv2.CustomerMarketPlaceId = inv1.CustomerMarketPlaceId)
  ) as TableInventoryCashValue) as fsda


FROM MP_CustomerMarketPlace cmp 

  
WHERE cmp.Id = @iCustomerMarketPlaceId and cmp.MarketPlaceId=@iMarketPlaceId
	
END
GO
