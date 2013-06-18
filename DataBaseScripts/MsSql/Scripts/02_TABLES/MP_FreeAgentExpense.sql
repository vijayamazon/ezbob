IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_FreeAgentExpense]') AND type in (N'U'))
DROP TABLE [dbo].[MP_FreeAgentExpense]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_FreeAgentExpense](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RequestId] [int] NOT NULL,
	[CategoryId] [int] NOT NULL,
	[url] [nvarchar](250) NULL,
	[username] [nvarchar](250) NULL,
	[category] [nvarchar](250) NULL,
	[dated_on] [datetime] NULL,
	[currency] [nvarchar](10) NULL,
	[gross_value] [numeric](18, 2) NULL,
	[native_gross_value] [numeric](18, 2) NULL,
	[sales_tax_rate] [numeric](18, 2) NULL,
	[sales_tax_value] [numeric](18, 2) NULL,
	[native_sales_tax_value] [numeric](18, 2) NULL,
	[description] [nvarchar](250) NULL,
	[manual_sales_tax_amount] [numeric](18, 2) NULL,
	[updated_at] [datetime] NULL,
	[created_at] [datetime] NULL,
	[attachment_url] [nvarchar](250) NULL,
	[attachment_content_src] [nvarchar](1000) NULL,
	[attachment_content_type] [nvarchar](250) NULL,
	[attachment_file_name] [nvarchar](250) NULL,
	[attachment_file_size] [int] NULL,
	[attachment_description] [nvarchar](250) NULL,
 CONSTRAINT [PK_MP_FreeAgentExpense] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_MP_FreeAgentExpenseCategoryId] ON [dbo].[MP_FreeAgentExpense] 
(
	[CategoryId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_MP_FreeAgentExpenseRequestId] ON [dbo].[MP_FreeAgentExpense] 
(
	[RequestId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
