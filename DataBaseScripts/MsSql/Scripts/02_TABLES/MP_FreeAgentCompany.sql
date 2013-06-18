IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_FreeAgentCompany]') AND type in (N'U'))
DROP TABLE [dbo].[MP_FreeAgentCompany]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_FreeAgentCompany](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RequestId] [int] NOT NULL,
	[url] [nvarchar](250) NULL,
	[name] [nvarchar](250) NULL,
	[subdomain] [nvarchar](250) NULL,
	[type] [nvarchar](250) NULL,
	[currency] [nvarchar](250) NULL,
	[mileage_units] [nvarchar](250) NULL,
	[company_start_date] [datetime] NULL,
	[freeagent_start_date] [datetime] NULL,
	[first_accounting_year_end] [datetime] NULL,
	[company_registration_number] [int] NULL,
	[sales_tax_registration_status] [nvarchar](250) NULL,
	[sales_tax_registration_number] [int] NULL,
 CONSTRAINT [PK_MP_FreeAgentCompany] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_MP_FreeAgentCompanyRequestId] ON [dbo].[MP_FreeAgentCompany] 
(
	[RequestId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
