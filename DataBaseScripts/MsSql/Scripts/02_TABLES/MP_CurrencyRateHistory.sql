IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_CurrencyRateHistory]') AND type in (N'U'))
DROP TABLE [dbo].[MP_CurrencyRateHistory]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_CurrencyRateHistory](
	[Id] [bigint] NOT NULL,
	[CurrencyId] [int] NOT NULL,
	[Price] [decimal](18, 8) NULL,
	[Updated] [datetime] NOT NULL,
 CONSTRAINT [PK_MP_CurrencyRateHistory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_MP_CurrencyRateHistory_CurId] ON [dbo].[MP_CurrencyRateHistory] 
(
	[CurrencyId] ASC
)
INCLUDE ( [Id],
[Price],
[Updated]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_MP_CurrencyRateHistory_UpdateId] ON [dbo].[MP_CurrencyRateHistory] 
(
	[CurrencyId] ASC,
	[Updated] ASC
)
INCLUDE ( [Price]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
