IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_FreeAgentInvoice]') AND type in (N'U'))
DROP TABLE [dbo].[MP_FreeAgentInvoice]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_FreeAgentInvoice](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RequestId] [int] NOT NULL,
	[url] [nvarchar](250) NULL,
	[contact] [nvarchar](250) NULL,
	[dated_on] [datetime] NULL,
	[due_on] [datetime] NULL,
	[reference] [nvarchar](250) NULL,
	[currency] [nvarchar](10) NULL,
	[exchange_rate] [numeric](18, 4) NULL,
	[net_value] [numeric](18, 2) NULL,
	[total_value] [numeric](18, 2) NULL,
	[paid_value] [numeric](18, 2) NULL,
	[due_value] [numeric](18, 2) NULL,
	[status] [nvarchar](250) NULL,
	[omit_header] [bit] NULL,
	[payment_terms_in_days] [int] NULL,
	[paid_on] [datetime] NULL,
 CONSTRAINT [PK_MP_FreeAgentInvoice] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_PK_MP_FreeAgentInvoiceRequestId] ON [dbo].[MP_FreeAgentInvoice] 
(
	[RequestId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
