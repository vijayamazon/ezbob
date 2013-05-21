IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_VolusionOrderItem]') AND type in (N'U'))
DROP TABLE [dbo].[MP_VolusionOrderItem]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_VolusionOrderItem](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OrderId] [int] NULL,
	[NativeOrderId] [nvarchar](300) NULL,
	[TotalCost] [numeric](18, 2) NULL,
	[CurrencyCode] [nvarchar](3) NULL,
	[PaymentDate] [datetime] NULL,
	[PurchaseDate] [datetime] NULL,
	[OrderStatus] [nvarchar](300) NULL,
 CONSTRAINT [PK_MP_VolusionOrderItem] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_MP_VolusionOrderItemOrderId] ON [dbo].[MP_VolusionOrderItem] 
(
	[OrderId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
