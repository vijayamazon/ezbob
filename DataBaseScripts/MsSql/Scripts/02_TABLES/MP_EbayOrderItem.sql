IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_EbayOrderItem]') AND type in (N'U'))
DROP TABLE [dbo].[MP_EbayOrderItem]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_EbayOrderItem](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OrderId] [int] NOT NULL,
	[AdjustmentAmount] [float] NULL,
	[AdjustmentCurrency] [nvarchar](50) NULL,
	[AmountPaidAmount] [float] NULL,
	[AmountPaidCurrency] [nvarchar](50) NULL,
	[SubTotalAmount] [float] NULL,
	[SubTotalCurrency] [nvarchar](50) NULL,
	[TotalAmount] [float] NULL,
	[TotalCurrency] [nvarchar](50) NULL,
	[PaymentStatus] [nvarchar](50) NULL,
	[PaymentMethod] [nvarchar](50) NULL,
	[CheckoutStatus] [nvarchar](50) NULL,
	[OrderStatus] [nvarchar](50) NULL,
	[PaymentHoldStatus] [nvarchar](50) NULL,
	[PaymentMethodsList] [nvarchar](256) NULL,
	[CreatedTime] [datetime] NULL,
	[PaymentTime] [datetime] NULL,
	[ShippedTime] [datetime] NULL,
	[BuyerName] [nvarchar](128) NULL,
	[ShippingAddressId] [int] NULL,
 CONSTRAINT [PK_MP_OrderItem] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_MP_EbayOrderItemOrderId] ON [dbo].[MP_EbayOrderItem] 
(
	[OrderId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_MP_OrderItemCreatedTime] ON [dbo].[MP_EbayOrderItem] 
(
	[CreatedTime] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_MP_OrderItemOrderStatus] ON [dbo].[MP_EbayOrderItem] 
(
	[OrderStatus] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
