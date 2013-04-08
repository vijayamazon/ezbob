IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_AmazonOrderItem]') AND type in (N'U'))
DROP TABLE [dbo].[MP_AmazonOrderItem]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_AmazonOrderItem](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AmazonOrderId] [int] NOT NULL,
	[OrderId] [nvarchar](128) NULL,
	[OrderItemId] [nvarchar](128) NULL,
	[PurchaseDate] [datetime] NULL,
	[PaymentsDate] [datetime] NULL,
	[BayerEmail] [nvarchar](128) NULL,
	[BayerName] [nvarchar](128) NULL,
	[BayerPhone] [nvarchar](128) NULL,
	[Sku] [nvarchar](128) NULL,
	[ProductName] [nvarchar](256) NULL,
	[QuantityPurchased] [int] NULL,
	[Currency] [nvarchar](50) NULL,
	[ItemPrice] [float] NULL,
	[ItemTax] [float] NULL,
	[RecipientName] [nvarchar](128) NULL,
	[SalesChennel] [nvarchar](128) NULL,
	[ShipStreet] [nvarchar](128) NULL,
	[ShipStreet1] [nvarchar](128) NULL,
	[ShipStreet2] [nvarchar](128) NULL,
	[ShipCityName] [nvarchar](128) NULL,
	[ShipStateOrProvince] [nvarchar](128) NULL,
	[ShipCountryName] [nvarchar](128) NULL,
	[ShipPostalCode] [nvarchar](50) NULL,
	[ShipPhone] [nvarchar](128) NULL,
	[ShipRecipient] [nvarchar](128) NULL,
	[ShipingPrice] [float] NULL,
	[ShipingTax] [float] NULL,
	[ShipServiceLevel] [nvarchar](128) NULL,
	[DeliveryStartDate] [datetime] NULL,
	[DeliveryEndDate] [datetime] NULL,
	[DeliveryTimeZone] [nvarchar](128) NULL,
	[DeliveryInstructions] [nvarchar](128) NULL,
 CONSTRAINT [PK_MP_AmazonOrderItem] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
