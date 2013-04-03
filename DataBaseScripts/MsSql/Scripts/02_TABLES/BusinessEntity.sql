IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BusinessEntity]') AND type in (N'U'))
DROP TABLE [dbo].[BusinessEntity]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BusinessEntity](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](25) NOT NULL,
	[Description] [nvarchar](1024) NULL,
	[Review] [nvarchar](1024) NOT NULL,
	[IsDeleted] [int] NULL,
	[CreationDate] [datetime] NOT NULL,
	[TerminationDate] [datetime] NULL,
	[UserId] [int] NOT NULL,
	[Document] [nvarchar](max) NOT NULL,
	[SignedDocument] [nvarchar](max) NULL,
	[SignedDocumentDelete] [nvarchar](max) NULL,
	[ItemVersion] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_BusinessEntity] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
