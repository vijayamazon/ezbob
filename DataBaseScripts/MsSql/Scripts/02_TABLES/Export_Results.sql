IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Export_Results]') AND type in (N'U'))
DROP TABLE [dbo].[Export_Results]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Export_Results](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FileName] [nvarchar](3000) NULL,
	[BinaryBody] [varbinary](max) NULL,
	[FileType] [int] NULL,
	[CreationDate] [datetime] NULL,
	[SourceTemplateId] [int] NULL,
	[ApplicationId] [bigint] NULL,
	[Status] [int] NULL,
	[StatusMode] [int] NULL,
	[NodeName] [nvarchar](500) NOT NULL,
	[SignedDocumentId] [bigint] NULL,
 CONSTRAINT [PK_Export_Results] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_Export_Results_FType] ON [dbo].[Export_Results] 
(
	[FileType] ASC,
	[ApplicationId] ASC
)
INCLUDE ( [SourceTemplateId]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Export_Results] ADD  CONSTRAINT [DF_Export_Results_CreationDate]  DEFAULT (getdate()) FOR [CreationDate]
GO
