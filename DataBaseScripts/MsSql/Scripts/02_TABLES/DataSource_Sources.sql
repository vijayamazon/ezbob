IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DataSource_Sources]') AND type in (N'U'))
DROP TABLE [dbo].[DataSource_Sources]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DataSource_Sources](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](500) NOT NULL,
	[Type] [nvarchar](20) NOT NULL,
	[Document] [nvarchar](max) NOT NULL,
	[SignedDocument] [nvarchar](max) NULL,
	[UserId] [int] NULL,
	[IsDeleted] [int] NULL,
	[CreationDate] [datetime] NULL,
	[TerminationDate] [datetime] NULL,
	[DisplayName] [nvarchar](255) NULL,
	[SignedDocumentDelete] [nvarchar](max) NULL,
 CONSTRAINT [PK_DATASOURCE_SOURCES] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
 CONSTRAINT [IX_DATASOURCE_SOURCES] UNIQUE NONCLUSTERED 
(
	[Name] ASC,
	[IsDeleted] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_1] ON [dbo].[DataSource_Sources] 
(
	[IsDeleted] ASC,
	[DisplayName] ASC,
	[TerminationDate] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
