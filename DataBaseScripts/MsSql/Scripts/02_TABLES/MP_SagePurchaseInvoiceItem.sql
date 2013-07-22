IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_SagePurchaseInvoiceItem]') AND type in (N'U'))
DROP TABLE [dbo].[MP_SagePurchaseInvoiceItem]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_SagePurchaseInvoiceItem](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PurchaseInvoiceId] [int] NOT NULL,
	[SageId] [int] NOT NULL,
	[description] [nvarchar](250) NULL,
	[quantity] [numeric](18, 2) NULL,
	[unit_price] [numeric](18, 2) NULL,
	[net_amount] [numeric](18, 2) NULL,
	[tax_amount] [numeric](18, 2) NULL,
	[TaxCodeId] [int] NULL,
	[tax_rate_percentage] [numeric](18, 2) NULL,
	[unit_price_includes_tax] [bit] NULL,
	[LedgerAccountId] [int] NULL,
	[product_code] [nvarchar](250) NULL,
	[ProductId] [int] NULL,
	[ServiceId] [int] NULL,
	[lock_version] [int] NULL,
 CONSTRAINT [PK_MP_SagePurchaseInvoiceItem] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_MP_SagePurchaseInvoiceItemPurchaseInvoiceId] ON [dbo].[MP_SagePurchaseInvoiceItem] 
(
	[PurchaseInvoiceId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
