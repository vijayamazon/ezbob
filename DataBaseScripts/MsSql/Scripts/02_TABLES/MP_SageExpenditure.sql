IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_SageExpenditure]') AND type in (N'U'))
DROP TABLE [dbo].[MP_SageExpenditure]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_SageExpenditure](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RequestId] [int] NOT NULL,
	[SageId] [int] NOT NULL,
	[date] [datetime] NULL,
	[invoice_date] [datetime] NULL,
	[amount] [numeric](18, 2) NULL,
	[tax_amount] [numeric](18, 2) NULL,
	[gross_amount] [numeric](18, 2) NULL,
	[tax_percentage_rate] [numeric](18, 2) NULL,
	[TaxCodeId] [int] NULL,
	[tax_scheme_period_id] [int] NULL,
	[reference] [nvarchar](250) NULL,
	[ContactId] [int] NULL,
	[SourceId] [int] NULL,
	[DestinationId] [int] NULL,
	[PaymentMethodId] [int] NULL,
	[voided] [bit] NULL,
	[lock_version] [int] NULL,
 CONSTRAINT [PK_MP_SageExpenditure] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_MP_SageExpenditureRequestId] ON [dbo].[MP_SageExpenditure] 
(
	[RequestId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
