IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Signal]') AND type in (N'U'))
DROP TABLE [dbo].[Signal]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Signal](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Target] [nvarchar](50) NOT NULL,
	[label] [nvarchar](250) NOT NULL,
	[Status] [int] NOT NULL,
	[StartTime] [datetime] NOT NULL,
	[AppSpecific] [int] NOT NULL,
	[ApplicationId] [bigint] NOT NULL,
	[Priority] [bigint] NULL,
	[ExecutionType] [smallint] NULL,
	[Message] [varbinary](max) NOT NULL,
	[OwnerApplicationId] [bigint] NULL,
	[IsExternal] [bit] NULL,
 CONSTRAINT [PK_Signal] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [i_signal_appid] ON [dbo].[Signal] 
(
	[ApplicationId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_Signal_IsExternal] ON [dbo].[Signal] 
(
	[IsExternal] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
