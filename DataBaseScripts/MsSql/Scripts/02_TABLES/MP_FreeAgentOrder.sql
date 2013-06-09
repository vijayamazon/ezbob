﻿IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_FreeAgentOrder]') AND type in (N'U'))
DROP TABLE [dbo].[MP_FreeAgentOrder]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_FreeAgentOrder](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerMarketPlaceId] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
	[CustomerMarketPlaceUpdatingHistoryRecordId] [int] NULL,
 CONSTRAINT [PK_MP_FreeAgentOrder] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_MP_FreeAgentOrderCustomerMarketPlaceId] ON [dbo].[MP_FreeAgentOrder] 
(
	[CustomerMarketPlaceId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
