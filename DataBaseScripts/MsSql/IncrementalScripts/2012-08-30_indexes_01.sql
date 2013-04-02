/*-----------*/
IF  EXISTS (SELECT * FROM sys.indexes 
            WHERE object_id = OBJECT_ID(N'[dbo].MP_EbayTransaction') 
            AND name = N'IX_MP_EbayTransaction_OItId')
DROP INDEX IX_MP_EbayTransaction_OItId ON [dbo].MP_EbayTransaction WITH ( ONLINE = OFF )
GO

CREATE NONCLUSTERED INDEX IX_MP_EbayTransaction_OItId
ON [dbo].MP_EbayTransaction ([OrderItemId])
INCLUDE ([ItemInfoId])
GO

/*-----------*/
IF  EXISTS (SELECT * FROM sys.indexes 
            WHERE object_id = OBJECT_ID(N'[dbo].MP_CurrencyRateHistory') 
            AND name = N'IX_MP_CurrencyRateHistory_CurId')
DROP INDEX IX_MP_CurrencyRateHistory_CurId ON [dbo].MP_CurrencyRateHistory WITH ( ONLINE = OFF )
GO


CREATE NONCLUSTERED INDEX IX_MP_CurrencyRateHistory_CurId
ON [dbo].[MP_CurrencyRateHistory] ([CurrencyId])
INCLUDE ([Id],[Price],[Updated])
GO

/*-----------*/
IF  EXISTS (SELECT * FROM sys.indexes 
            WHERE object_id = OBJECT_ID(N'[dbo].MP_AmazonOrderItem2') 
            AND name = N'IX_MP_AmazonOrderItem2_AOId')
DROP INDEX IX_MP_AmazonOrderItem2_AOId ON [dbo].MP_AmazonOrderItem2 WITH ( ONLINE = OFF )
GO

CREATE NONCLUSTERED INDEX IX_MP_AmazonOrderItem2_AOId
ON [dbo].[MP_AmazonOrderItem2] ([AmazonOrderId])
INCLUDE ([Id],[OrderId],[SellerOrderId],[PurchaseDate],
[LastUpdateDate],[OrderStatus],[FulfillmentChannel],[SalesChannel],
[OrderChannel],[ShipServiceLevel],[OrderTotalCurrency],[OrderTotal],
[PaymentMethod],[BuyerName],[ShipmentServiceLevelCategory],[BuyerEmail],
[NumberOfItemsShipped],[NumberOfItemsUnshipped],[MarketplaceId],[ShipAddress1],
[ShipAddress2],[ShipAddress3],[ShipCity],[ShipCountryCode],[ShipCounty],[ShipDistrict],
[ShipName],[ShipPhone],[PostalCode],[StateOrRegion])

GO

