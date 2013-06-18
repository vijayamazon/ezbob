IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_FreeAgentExpenseCategory]') AND type in (N'U'))
DROP TABLE [dbo].[MP_FreeAgentExpenseCategory]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_FreeAgentExpenseCategory](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[url] [nvarchar](250) NULL,
	[description] [nvarchar](250) NULL,
	[nominal_code] [nvarchar](250) NULL,
	[allowable_for_tax] [bit] NULL,
	[tax_reporting_name] [nvarchar](250) NULL,
	[auto_sales_tax_rate] [nvarchar](250) NULL,
	[category_group] [nvarchar](250) NULL,
 CONSTRAINT [PK_MP_FreeAgentExpenseCategory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
 CONSTRAINT [IX_MP_FreeAgentExpenseCategory_url] UNIQUE NONCLUSTERED 
(
	[url] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_MP_FreeAgentExpenseCategoryurl] ON [dbo].[MP_FreeAgentExpenseCategory] 
(
	[url] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
