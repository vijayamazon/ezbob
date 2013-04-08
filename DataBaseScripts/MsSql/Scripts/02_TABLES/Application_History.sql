IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Application_History]') AND type in (N'U'))
DROP TABLE [dbo].[Application_History]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Application_History](
	[AppHistoryId] [bigint] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NULL,
	[SecurityApplicationId] [int] NULL,
	[ActionDateTime] [datetime] NULL,
	[ActionType] [int] NULL,
	[ApplicationId] [bigint] NULL,
	[CurrentNodeID] [int] NULL,
 CONSTRAINT [PK_Application_History] PRIMARY KEY CLUSTERED 
(
	[AppHistoryId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [i_app_hist_1] ON [dbo].[Application_History] 
(
	[UserId] ASC
)
INCLUDE ( [AppHistoryId],
[ActionDateTime],
[CurrentNodeID],
[ActionType],
[ApplicationId],
[SecurityApplicationId]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [i_app_hist_2] ON [dbo].[Application_History] 
(
	[SecurityApplicationId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [i_app_hist_3] ON [dbo].[Application_History] 
(
	[ApplicationId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
