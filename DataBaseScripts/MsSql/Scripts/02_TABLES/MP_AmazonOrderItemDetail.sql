IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_AmazonOrderItemDetail]') AND type in (N'U'))
DROP TABLE [dbo].[MP_AmazonOrderItemDetail]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_AmazonOrderItemDetail](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OrderItem2Id] [int] NULL,
	[SellerSKU] [nvarchar](50) NULL,
	[AmazonOrderItemId] [nvarchar](50) NULL,
	[ASIN] [nvarchar](50) NULL,
	[CODFeeCurrency] [nvarchar](10) NULL,
	[CODFeePrice] [float] NULL,
	[CODFeeDiscountCurrency] [nvarchar](10) NULL,
	[CODFeeDiscountPrice] [float] NULL,
	[GiftMessageText] [nvarchar](max) NULL,
	[GiftWrapLevel] [nvarchar](50) NULL,
	[GiftWrapPriceCurrency] [nvarchar](10) NULL,
	[GiftWrapPrice] [float] NULL,
	[GiftWrapTaxCurrency] [nvarchar](10) NULL,
	[GiftWrapTaxPrice] [float] NULL,
	[ItemPriceCurrency] [nvarchar](10) NULL,
	[ItemPrice] [float] NULL,
	[ItemTaxCurrency] [nvarchar](10) NULL,
	[ItemTaxPrice] [float] NULL,
	[PromotionDiscountCurrency] [nvarchar](10) NULL,
	[PromotionDiscountPrice] [float] NULL,
	[QuantityOrdered] [int] NULL,
	[QuantityShipped] [int] NULL,
	[ShippingDiscountCurrency] [nvarchar](10) NULL,
	[ShippingDiscountPrice] [float] NULL,
	[ShippingPriceCurrency] [nvarchar](10) NULL,
	[ShippingPrice] [float] NULL,
	[ShippingTaxCurrency] [nvarchar](10) NULL,
	[ShippingTaxPrice] [float] NULL,
	[Title] [nvarchar](max) NULL,
 CONSTRAINT [PK_MP_AmazonOrderItemDetail] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_MP_AmazonOrderItemDetail_ASIN] ON [dbo].[MP_AmazonOrderItemDetail] 
(
	[ASIN] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
