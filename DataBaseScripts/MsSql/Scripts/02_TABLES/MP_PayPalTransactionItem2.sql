IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_PayPalTransactionItem2]') AND type in (N'U'))
DROP TABLE [dbo].[MP_PayPalTransactionItem2]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_PayPalTransactionItem2](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TransactionId] [int] NOT NULL,
	[Created] [datetime] NULL,
	[CurrencyId] [int] NULL,
	[FeeAmount] [float] NULL,
	[GrossAmount] [float] NULL,
	[NetAmount] [float] NULL,
	[TimeZone] [nvarchar](128) NULL,
	[Type] [nvarchar](128) NULL,
	[Status] [nvarchar](128) NULL,
	[PayPalTransactionId] [nvarchar](128) NULL,
 CONSTRAINT [PK_MP_TransactionItem2] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_MP_PayPalTransactionItem2_Type] ON [dbo].[MP_PayPalTransactionItem2] 
(
	[TransactionId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [MP_PayPalTransactionItem2_TI] ON [dbo].[MP_PayPalTransactionItem2] 
(
	[Created] ASC,
	[Type] ASC,
	[Status] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
