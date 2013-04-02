
ALTER TABLE [dbo].[MP_CurrencyRateHistory] DROP CONSTRAINT [FK_MP_CurrencyRateHistory_MP_Currency]
GO

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
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[MP_CurrencyRateHistory]  WITH CHECK ADD  CONSTRAINT [FK_MP_CurrencyRateHistory_MP_Currency] FOREIGN KEY([CurrencyId])
REFERENCES [dbo].[MP_Currency] ([Id])
GO

ALTER TABLE [dbo].[MP_CurrencyRateHistory] CHECK CONSTRAINT [FK_MP_CurrencyRateHistory_MP_Currency]
GO