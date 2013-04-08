IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_EbayTransaction]') AND type in (N'U'))
DROP TABLE [dbo].[MP_EbayTransaction]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_EbayTransaction](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OrderItemId] [int] NOT NULL,
	[CreatedDate] [datetime] NULL,
	[QuantityPurchased] [int] NULL,
	[PaymentHoldStatus] [nvarchar](50) NULL,
	[PaymentMethodUsed] [nvarchar](50) NULL,
	[Price] [float] NULL,
	[PriceCurrency] [nvarchar](50) NULL,
	[ItemID] [nvarchar](128) NULL,
	[ItemPrivateNotes] [nvarchar](max) NULL,
	[ItemSellerInventoryID] [nvarchar](128) NULL,
	[ItemSKU] [nvarchar](128) NULL,
	[eBayTransactionId] [nvarchar](128) NULL,
	[ItemInfoId] [int] NULL,
 CONSTRAINT [PK_MP_EbayTransaction] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_MP_EbayTransaction_OItId] ON [dbo].[MP_EbayTransaction] 
(
	[OrderItemId] ASC
)
INCLUDE ( [ItemInfoId]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
