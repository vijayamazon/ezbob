IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_Strategy]') AND type in (N'U'))
DROP TABLE [dbo].[Strategy_Strategy]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Strategy_Strategy](
	[StrategyId] [int] IDENTITY(30,1) NOT NULL,
	[CurrentVersionId] [int] NULL,
	[Name] [nvarchar](400) NOT NULL,
	[Description] [nvarchar](500) NULL,
	[IsEmbeddingAllowed] [bit] NOT NULL,
	[XML] [ntext] NOT NULL,
	[IsDeleted] [int] NOT NULL,
	[UserId] [int] NULL,
	[AuthorId] [int] NOT NULL,
	[State] [tinyint] NOT NULL,
	[SubState] [tinyint] NOT NULL,
	[CreationDate] [datetime] NOT NULL,
	[Icon] [image] NULL,
	[ExecutionDuration] [bigint] NULL,
	[LastUpdateDate] [datetime] NULL,
	[StrategyType] [nvarchar](255) NOT NULL,
	[DisplayName] [nvarchar](350) NULL,
	[TermDate] [datetime] NULL,
	[SignedDocument] [ntext] NULL,
	[IsMigrationSupported] [bit] NULL,
	[SignedDocumentDelete] [ntext] NULL,
	[InDbFormat] [varbinary](max) NULL,
 CONSTRAINT [PK_Strategy_Strategy] PRIMARY KEY CLUSTERED 
(
	[StrategyId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
 CONSTRAINT [IX_Strategy_Strategy] UNIQUE NONCLUSTERED 
(
	[Name] ASC,
	[IsDeleted] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[Strategy_Strategy] ADD  CONSTRAINT [DF_Strategy_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Strategy_Strategy] ADD  CONSTRAINT [DF_Strategy_State]  DEFAULT ((0)) FOR [State]
GO
ALTER TABLE [dbo].[Strategy_Strategy] ADD  CONSTRAINT [DF_Strategy_SubState]  DEFAULT ((1)) FOR [SubState]
GO
ALTER TABLE [dbo].[Strategy_Strategy] ADD  CONSTRAINT [DF_Strategy_CreationDate]  DEFAULT (getdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[Strategy_Strategy] ADD  CONSTRAINT [DF_Strategy_Strategy_LastUpdateDate]  DEFAULT (getdate()) FOR [LastUpdateDate]
GO
