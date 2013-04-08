IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_EkmOrderItem]') AND type in (N'U'))
DROP TABLE [dbo].[MP_EkmOrderItem]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_EkmOrderItem](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OrderId] [int] NULL,
	[OrderNumber] [nvarchar](300) NULL,
	[CustomerId] [int] NULL,
	[CompanyName] [nvarchar](300) NULL,
	[FirstName] [nvarchar](300) NULL,
	[LastName] [nvarchar](300) NULL,
	[EmailAddress] [nvarchar](300) NULL,
	[TotalCost] [numeric](18, 2) NULL,
	[OrderDate] [datetime] NULL,
	[OrderDateIso] [datetime] NULL,
	[OrderStatus] [nvarchar](300) NULL,
	[OrderStatusColour] [nvarchar](300) NULL,
 CONSTRAINT [PK_MP_EkmOrderItem] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_MP_EkmOrderItemOrderId] ON [dbo].[MP_EkmOrderItem] 
(
	[OrderId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
