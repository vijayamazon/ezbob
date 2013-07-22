IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_SagePurchaseInvoice]') AND type in (N'U'))
DROP TABLE [dbo].[MP_SagePurchaseInvoice]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_SagePurchaseInvoice](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RequestId] [int] NOT NULL,
	[SageId] [int] NOT NULL,
	[StatusId] [int] NULL,
	[due_date] [datetime] NULL,
	[date] [datetime] NULL,
	[void_reason] [nvarchar](250) NULL,
	[outstanding_amount] [numeric](18, 2) NULL,
	[total_net_amount] [numeric](18, 2) NULL,
	[total_tax_amount] [numeric](18, 2) NULL,
	[tax_scheme_period_id] [int] NULL,
	[ContactId] [int] NULL,
	[contact_name] [nvarchar](250) NULL,
	[main_address] [nvarchar](250) NULL,
	[delivery_address] [nvarchar](250) NULL,
	[delivery_address_same_as_main] [bit] NULL,
	[reference] [nvarchar](250) NULL,
	[notes] [nvarchar](250) NULL,
	[terms_and_conditions] [nvarchar](250) NULL,
	[lock_version] [int] NULL,
 CONSTRAINT [PK_MP_SagePurchaseInvoice] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_MP_SagePurchaseInvoiceRequestId] ON [dbo].[MP_SagePurchaseInvoice] 
(
	[RequestId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
