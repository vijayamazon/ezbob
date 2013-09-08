IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_AnalyisisFunctionValues]') AND type in (N'U'))
DROP TABLE [dbo].[MP_AnalyisisFunctionValues]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_AnalyisisFunctionValues](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Updated] [datetime] NOT NULL,
	[CustomerMarketPlaceId] [int] NOT NULL,
	[AnalyisisFunctionId] [int] NOT NULL,
	[AnalysisFunctionTimePeriodId] [int] NOT NULL,
	[ValueString] [nvarchar](max) NULL,
	[ValueInt] [int] NULL,
	[ValueFloat] [float] NULL,
	[ValueDate] [datetime] NULL,
	[Value] [nvarchar](max) NULL,
	[ValueBoolean] [bit] NULL,
	[ValueXml] [nvarchar](max) NULL,
	[CustomerMarketPlaceUpdatingHistoryRecordId] [int] NULL,
 CONSTRAINT [PK_AnalyisisFunctionValues] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_AnalyisisFunctionCreated] ON [dbo].[MP_AnalyisisFunctionValues] 
(
	[Updated] DESC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_MP_AnalyisisFunctionValues] ON [dbo].[MP_AnalyisisFunctionValues] 
(
	[AnalyisisFunctionId] ASC,
	[AnalysisFunctionTimePeriodId] ASC,
	[CustomerMarketPlaceId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_MP_AnalyisisFunctionValues_AFI] ON [dbo].[MP_AnalyisisFunctionValues] 
(
	[AnalyisisFunctionId] ASC
)
INCLUDE ( [AnalysisFunctionTimePeriodId],
[ValueFloat],
[CustomerMarketPlaceUpdatingHistoryRecordId]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_MP_AnalyisisFunctionValues_AFTPI] ON [dbo].[MP_AnalyisisFunctionValues] 
(
	[AnalysisFunctionTimePeriodId] ASC
)
INCLUDE ( [AnalyisisFunctionId],
[ValueInt],
[CustomerMarketPlaceUpdatingHistoryRecordId]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_MP_AnalyisisFunctionValues_CustomerMarketPlaceUpdatingHistoryRecordId] ON [dbo].[MP_AnalyisisFunctionValues] 
(
	[CustomerMarketPlaceUpdatingHistoryRecordId] ASC
)
INCLUDE ( [AnalyisisFunctionId],
[AnalysisFunctionTimePeriodId],
[ValueInt],
[ValueFloat]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_MP_AnalyisisFunctionValues_Include] ON [dbo].[MP_AnalyisisFunctionValues] 
(
	[AnalyisisFunctionId] ASC
)
INCLUDE ( [AnalysisFunctionTimePeriodId],
[ValueInt],
[ValueFloat],
[CustomerMarketPlaceUpdatingHistoryRecordId]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_MP_AnalyisisFunctionValues2] ON [dbo].[MP_AnalyisisFunctionValues] 
(
	[CustomerMarketPlaceId] ASC,
	[AnalyisisFunctionId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [MP_AnalyisisFunctionValues_AFTP] ON [dbo].[MP_AnalyisisFunctionValues] 
(
	[AnalysisFunctionTimePeriodId] ASC
)
INCLUDE ( [AnalyisisFunctionId],
[ValueInt],
[CustomerMarketPlaceUpdatingHistoryRecordId]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [MP_AnalyisisFunctionValues_Include] ON [dbo].[MP_AnalyisisFunctionValues] 
(
	[AnalyisisFunctionId] ASC
)
INCLUDE ( [AnalysisFunctionTimePeriodId],
[ValueInt],
[ValueFloat],
[CustomerMarketPlaceUpdatingHistoryRecordId]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
