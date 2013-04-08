IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ScoringModel]') AND type in (N'U'))
DROP TABLE [dbo].[ScoringModel]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ScoringModel](
	[Id] [int] NOT NULL,
	[Guid] [nvarchar](1024) NULL,
	[DisplayName] [nvarchar](1024) NULL,
	[UserId] [int] NULL,
	[ModelTypeName] [nvarchar](250) NULL,
	[CreationDate] [datetime] NULL,
	[IsDeleted] [int] NULL,
	[TerminationDate] [datetime] NULL,
	[Description] [nvarchar](1024) NULL,
	[Cutoffpoint] [decimal](5, 5) NULL,
	[AllowWeightsEdit] [int] NULL,
	[AllowSaveResults] [int] NULL,
	[PmmlFile] [nvarchar](max) NULL,
	[SignedDocument] [nvarchar](max) NULL,
	[SignedDocumentDelete] [nvarchar](max) NULL,
 CONSTRAINT [PK_Models] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
