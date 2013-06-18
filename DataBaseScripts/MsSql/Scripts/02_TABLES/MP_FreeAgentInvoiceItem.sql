IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_FreeAgentInvoiceItem]') AND type in (N'U'))
DROP TABLE [dbo].[MP_FreeAgentInvoiceItem]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_FreeAgentInvoiceItem](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[InvoiceId] [int] NOT NULL,
	[url] [nvarchar](250) NULL,
	[position] [int] NULL,
	[description] [nvarchar](250) NULL,
	[item_type] [nvarchar](250) NULL,
	[price] [numeric](18, 2) NULL,
	[quantity] [numeric](18, 2) NULL,
	[category] [nvarchar](250) NULL,
 CONSTRAINT [PK_MP_FreeAgentInvoiceItem] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_MP_FreeAgentInvoiceItemInvoiceId] ON [dbo].[MP_FreeAgentInvoiceItem] 
(
	[InvoiceId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
