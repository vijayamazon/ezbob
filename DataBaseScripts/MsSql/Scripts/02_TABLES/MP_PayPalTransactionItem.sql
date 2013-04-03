IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_PayPalTransactionItem]') AND type in (N'U'))
DROP TABLE [dbo].[MP_PayPalTransactionItem]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_PayPalTransactionItem](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TransactionId] [int] NOT NULL,
	[Created] [datetime] NULL,
	[FeeAmountCurrency] [nvarchar](50) NULL,
	[FeeAmountAmount] [float] NULL,
	[GrossAmountCurrency] [nvarchar](50) NULL,
	[GrossAmountAmount] [float] NULL,
	[NetAmountCurrency] [nvarchar](50) NULL,
	[NetAmountAmount] [float] NULL,
	[TimeZone] [nvarchar](128) NULL,
	[Type] [nvarchar](128) NULL,
	[Status] [nvarchar](128) NULL,
	[Payer] [nvarchar](128) NULL,
	[PayerDisplayName] [nvarchar](128) NULL,
	[PayPalTransactionId] [nvarchar](128) NULL,
 CONSTRAINT [PK_MP_TransactionItem] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_MP_PayPalTransactionItem_Payer] ON [dbo].[MP_PayPalTransactionItem] 
(
	[Payer] ASC
)
INCLUDE ( [TransactionId],
[Created],
[NetAmountAmount],
[Type],
[Status]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_MP_PayPalTransactionItem_Type] ON [dbo].[MP_PayPalTransactionItem] 
(
	[TransactionId] ASC,
	[Type] ASC,
	[Status] ASC,
	[NetAmountAmount] ASC
)
INCLUDE ( [Payer],
[Created],
[FeeAmountCurrency],
[FeeAmountAmount],
[GrossAmountCurrency],
[GrossAmountAmount],
[NetAmountCurrency],
[TimeZone],
[PayerDisplayName],
[PayPalTransactionId]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [MP_PayPalTransactionItem_TI] ON [dbo].[MP_PayPalTransactionItem] 
(
	[TransactionId] ASC
)
INCLUDE ( [Id],
[Created],
[FeeAmountCurrency],
[FeeAmountAmount],
[GrossAmountCurrency],
[GrossAmountAmount],
[NetAmountCurrency],
[NetAmountAmount],
[TimeZone],
[Type],
[Status],
[Payer],
[PayerDisplayName],
[PayPalTransactionId]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
