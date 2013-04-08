IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Application_HistorySumm]') AND type in (N'U'))
DROP TABLE [dbo].[Application_HistorySumm]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Application_HistorySumm](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ApplicationId] [int] NULL,
	[NodeId] [int] NULL,
	[SecurityApplicationId] [int] NULL,
	[SummFirstTimeOutage] [int] NULL,
	[SummOutageTimeLockUnlock] [int] NULL,
	[GeneralOutageTime] [int] NULL,
	[SummTimeOfFly] [int] NULL,
	[SummWorkTime] [int] NULL,
	[FirstComingTime] [datetime] NULL,
	[LastExitTime] [datetime] NULL,
	[AbsoluteTimeOfFlyNode] [int] NULL,
 CONSTRAINT [PK_Application_HistorySumm] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_Application_HistorySumm] ON [dbo].[Application_HistorySumm] 
(
	[ApplicationId] ASC,
	[NodeId] ASC
)
INCLUDE ( [SecurityApplicationId],
[Id]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
