IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_EbayExternalTransaction]') AND type in (N'U'))
DROP TABLE [dbo].[MP_EbayExternalTransaction]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_EbayExternalTransaction](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OrderItemId] [int] NULL,
	[TransactionID] [nvarchar](128) NULL,
	[TransactionTime] [datetime] NULL,
	[FeeOrCreditCurrency] [nvarchar](50) NULL,
	[FeeOrCreditPrice] [float] NULL,
	[PaymentOrRefundACurrency] [nvarchar](50) NULL,
	[PaymentOrRefundAPrice] [float] NULL,
 CONSTRAINT [PK_MP_EbayExternalTransaction] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
