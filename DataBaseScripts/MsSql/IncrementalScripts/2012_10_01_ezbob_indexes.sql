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

/****** Object:  Index [IX_MP_CurrencyRateHistory_UpdateId]    Script Date: 09/18/2012 10:01:53 ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[MP_CurrencyRateHistory]')
AND name = N'IX_MP_CurrencyRateHistory_UpdateId')
DROP INDEX [IX_MP_CurrencyRateHistory_UpdateId] ON [dbo].[MP_CurrencyRateHistory] WITH ( ONLINE = OFF )
GO

/****** Object:  Index [IX_MP_CurrencyRateHistory_UpdateId]    Script Date: 09/18/2012 10:01:53 ******/
CREATE NONCLUSTERED INDEX [IX_MP_CurrencyRateHistory_UpdateId] ON [dbo].[MP_CurrencyRateHistory] 
(
	[CurrencyId] ASC,
	[Updated] ASC
)
INCLUDE ( [Price])
GO


/****** Object:  Index [IX_MP_CustomerMarketPlace]    Script Date: 09/18/2012 10:09:12 ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[MP_CustomerMarketPlace]')
AND name = N'IX_MP_CustomerMarketPlace')
DROP INDEX [IX_MP_CustomerMarketPlace] ON [dbo].[MP_CustomerMarketPlace] WITH ( ONLINE = OFF )
GO

/****** Object:  Index [IX_MP_CustomerMarketPlace]    Script Date: 09/18/2012 10:09:12 ******/
CREATE NONCLUSTERED INDEX [IX_MP_CustomerMarketPlace] ON [dbo].[MP_CustomerMarketPlace] 
(
	[CustomerId] ASC
)
INCLUDE ( [MarketPlaceId],
[DisplayName])
GO

/****** Object:  Index [IX_MP_EbayAmazonInventoryItem_Id]    Script Date: 09/18/2012 10:19:32 ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[MP_EbayAmazonInventoryItem]') 
AND name = N'IX_MP_EbayAmazonInventoryItem_Id')
DROP INDEX [IX_MP_EbayAmazonInventoryItem_Id] ON [dbo].[MP_EbayAmazonInventoryItem] WITH ( ONLINE = OFF )
GO


/****** Object:  Index [IX_MP_EbayAmazonInventoryItem_Id]    Script Date: 09/18/2012 10:19:32 ******/
CREATE NONCLUSTERED INDEX [IX_MP_EbayAmazonInventoryItem_Id] ON [dbo].[MP_EbayAmazonInventoryItem] 
(
	[InventoryId] ASC
)
INCLUDE ( [Price],
[Quantity],
[Currency])
GO

/*-------------------*/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[MP_Alert]') 
AND name = N'IX_MP_Alert_CustId')
DROP INDEX [IX_MP_Alert_CustId] ON [dbo].[MP_Alert] WITH ( ONLINE = OFF )
GO

CREATE NONCLUSTERED INDEX [IX_MP_Alert_CustId] ON [dbo].[MP_Alert] 
(
	[CustomerId] ASC
)
INCLUDE ( [StrategyStartedDate]) 
GO


/*-----------------*/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Export_Results]') 
AND name = N'IX_Export_Results_FType')
DROP INDEX [IX_Export_Results_FType] ON [dbo].[Export_Results] WITH ( ONLINE = OFF )
GO

CREATE NONCLUSTERED INDEX [IX_Export_Results_FType] ON [dbo].[Export_Results] 
(
	[FileType] ASC,
	[ApplicationId] ASC
)
INCLUDE ( [SourceTemplateId])
GO