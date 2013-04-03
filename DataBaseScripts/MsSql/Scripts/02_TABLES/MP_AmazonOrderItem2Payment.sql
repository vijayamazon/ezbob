IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_AmazonOrderItem2Payment]') AND type in (N'U'))
DROP TABLE [dbo].[MP_AmazonOrderItem2Payment]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_AmazonOrderItem2Payment](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OrderItem2Id] [int] NULL,
	[SubPaymentMethod] [nvarchar](50) NULL,
	[MoneyInfoCurrency] [nvarchar](50) NULL,
	[MoneyInfoAmount] [decimal](18, 8) NULL,
 CONSTRAINT [PK_MP_AmazonOrderItem2Payment] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
