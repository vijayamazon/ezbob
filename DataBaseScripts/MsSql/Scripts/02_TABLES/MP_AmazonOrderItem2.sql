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
	[OrderTotalCurrency] [nvarchar](50) NULL,
	[OrderTotal] [decimal](18, 8) NULL,
	[NumberOfItemsShipped] [int] NULL,
	[NumberOfItemsUnshipped] [int] NULL,
 CONSTRAINT [PK_MP_AmazonOrderItem2] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_AMO2_MarketPlaceId_OS] ON [dbo].[MP_AmazonOrderItem2] 
(
	[OrderStatus] ASC,
	[Id] ASC,
	[AmazonOrderId] ASC,
	[PurchaseDate] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_AMO2_MarketPlaceId_PD] ON [dbo].[MP_AmazonOrderItem2] 
(
	[AmazonOrderId] ASC,
	[PurchaseDate] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_AMO2_MarketPlaceId_PD2] ON [dbo].[MP_AmazonOrderItem2] 
(
	[AmazonOrderId] ASC,
	[PurchaseDate] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_MP_AmazonOrderItem2_AOId] ON [dbo].[MP_AmazonOrderItem2] 
(
	[AmazonOrderId] ASC,
	[Id] ASC,
	[OrderId] ASC,
	[SellerOrderId] ASC,
	[PurchaseDate] ASC,
	[LastUpdateDate] ASC,
	[OrderStatus] ASC,
	[OrderTotalCurrency] ASC,
	[OrderTotal] ASC,
	[NumberOfItemsShipped] ASC,
	[NumberOfItemsUnshipped] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
