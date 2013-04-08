IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_Node]') AND type in (N'U'))
DROP TABLE [dbo].[Strategy_Node]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Strategy_Node](
	[NodeId] [int] IDENTITY(1,1) NOT NULL,
	[GroupId] [tinyint] NOT NULL,
	[Name] [nvarchar](512) NOT NULL,
	[DisplayName] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](500) NULL,
	[ExecutionDuration] [bigint] NULL,
	[Icon] [image] NOT NULL,
	[IsDeleted] [int] NOT NULL,
	[ApplicationId] [int] NULL,
	[IsHardReaction] [bit] NOT NULL,
	[ContainsPrint] [int] NULL,
	[CustomUrl] [nvarchar](4000) NULL,
	[StartDate] [datetime] NULL,
	[Guid] [nvarchar](256) NULL,
	[CreatorUserId] [int] NOT NULL,
	[DeleterUserId] [int] NULL,
	[NodeComment] [nvarchar](1024) NULL,
	[TerminationDate] [datetime] NULL,
	[NDX] [image] NOT NULL,
	[SignedDocument] [ntext] NULL,
	[SignedDocumentDelete] [ntext] NULL,
 CONSTRAINT [PK_Strategy_Node] PRIMARY KEY CLUSTERED 
(
	[NodeId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Strategy_Node] ON [dbo].[Strategy_Node] 
(
	[Guid] ASC,
	[IsDeleted] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Strategy_Node] ADD  CONSTRAINT [DF_Strategy_Node_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Strategy_Node] ADD  DEFAULT ((0)) FOR [IsHardReaction]
GO
ALTER TABLE [dbo].[Strategy_Node] ADD  CONSTRAINT [DF_Strategy_Node_StartDate]  DEFAULT (getdate()) FOR [StartDate]
GO
