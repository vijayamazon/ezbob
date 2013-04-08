IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_AmazonOrderItem2]') AND type in (N'U'))
DROP TABLE [dbo].[MP_AmazonOrderItem2]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_AmazonOrderItem2](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AmazonOrderId] [int] NULL,
	[OrderId] [nvarchar](50) NULL,
	[SellerOrderId] [nvarchar](50) NULL,
	[PurchaseDate] [datetime] NULL,
	[LastUpdateDate] [datetime] NULL,
	[OrderStatus] [nvarchar](50) NULL,
	[FulfillmentChannel] [nvarchar](50) NULL,
	[SalesChannel] [nvarchar](50) NULL,
	[OrderChannel] [nvarchar](50) NULL,
	[ShipServiceLevel] [nvarchar](50) NULL,
	[OrderTotalCurrency] [nvarchar](50) NULL,
	[OrderTotal] [decimal](18, 8) NULL,
	[PaymentMethod] [nvarchar](50) NULL,
	[BuyerName] [nvarchar](128) NULL,
	[ShipmentServiceLevelCategory] [nvarchar](50) NULL,
	[BuyerEmail] [nvarchar](128) NULL,
	[NumberOfItemsShipped] [int] NULL,
	[NumberOfItemsUnshipped] [int] NULL,
	[MarketplaceId] [nvarchar](50) NULL,
	[ShipAddress1] [nvarchar](128) NULL,
	[ShipAddress2] [nvarchar](128) NULL,
	[ShipAddress3] [nvarchar](128) NULL,
	[ShipCity] [nvarchar](50) NULL,
	[ShipCountryCode] [nvarchar](50) NULL,
	[ShipCounty] [nvarchar](50) NULL,
	[ShipDistrict] [nvarchar](50) NULL,
	[ShipName] [nvarchar](50) NULL,
	[ShipPhone] [nvarchar](50) NULL,
	[PostalCode] [nvarchar](50) NULL,
	[StateOrRegion] [nvarchar](50) NULL,
 CONSTRAINT [PK_MP_AmazonOrderItem2] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_AMO2_MarketPlaceID] ON [dbo].[MP_AmazonOrderItem2] 
(
	[MarketplaceId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_AMO2_MarketPlaceId_OS] ON [dbo].[MP_AmazonOrderItem2] 
(
	[OrderStatus] ASC,
	[MarketplaceId] ASC
)
INCLUDE ( [Id],
[AmazonOrderId],
[PurchaseDate]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_AMO2_MarketPlaceId_PD] ON [dbo].[MP_AmazonOrderItem2] 
(
	[MarketplaceId] ASC
)
INCLUDE ( [AmazonOrderId],
[PurchaseDate]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_AMO2_MarketPlaceId_PD2] ON [dbo].[MP_AmazonOrderItem2] 
(
	[AmazonOrderId] ASC,
	[MarketplaceId] ASC
)
INCLUDE ( [PurchaseDate]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_MP_AmazonOrderItem2_AOId] ON [dbo].[MP_AmazonOrderItem2] 
(
	[AmazonOrderId] ASC
)
INCLUDE ( [Id],
[OrderId],
[SellerOrderId],
[PurchaseDate],
[LastUpdateDate],
[OrderStatus],
[FulfillmentChannel],
[SalesChannel],
[OrderChannel],
[ShipServiceLevel],
[OrderTotalCurrency],
[OrderTotal],
[PaymentMethod],
[BuyerName],
[ShipmentServiceLevelCategory],
[BuyerEmail],
[NumberOfItemsShipped],
[NumberOfItemsUnshipped],
[MarketplaceId],
[ShipAddress1],
[ShipAddress2],
[ShipAddress3],
[ShipCity],
[ShipCountryCode],
[ShipCounty],
[ShipDistrict],
[ShipName],
[ShipPhone],
[PostalCode],
[StateOrRegion]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
