IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_PublicName]') AND type in (N'U'))
DROP TABLE [dbo].[Strategy_PublicName]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Strategy_PublicName](
	[PUBLICNAMEID] [int] IDENTITY(1,1) NOT NULL,
	[NAME] [nvarchar](255) NOT NULL,
	[ISSTOPPED] [int] NULL,
	[IsDeleted] [int] NOT NULL,
	[DeleterUserId] [int] NULL,
	[TerminationDate] [datetime] NULL,
	[SignedDocumentDelete] [ntext] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[Strategy_PublicName] ADD  CONSTRAINT [DF_PublicName_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
