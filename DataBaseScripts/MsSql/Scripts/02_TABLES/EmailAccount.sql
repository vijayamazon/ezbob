IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EmailAccount]') AND type in (N'U'))
DROP TABLE [dbo].[EmailAccount]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EmailAccount](
	[Id] [int] NOT NULL,
	[IsDeleted] [int] NULL,
	[Type] [nvarchar](256) NOT NULL,
	[Name] [nvarchar](1024) NULL,
	[Description] [nvarchar](1024) NULL,
	[EmailFrom] [nvarchar](1024) NULL,
	[ServerAddress] [nvarchar](1024) NULL,
	[SmtpServerAddress] [nvarchar](1024) NULL,
	[Port] [int] NULL,
	[UserName] [nvarchar](1024) NULL,
	[Password] [nvarchar](1024) NULL,
	[EncryptionType] [nvarchar](64) NULL,
	[RequireAuthenication] [bit] NULL,
	[SignedDocument] [nvarchar](max) NULL,
	[SignedDocumentDelete] [nvarchar](max) NULL,
	[TerminationDate] [datetime] NULL,
	[StartDate] [datetime] NULL,
	[CreatorUserId] [int] NULL,
 CONSTRAINT [PK_EmailAccount] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[EmailAccount] ADD  CONSTRAINT [DF_EmailAccount_StartDate]  DEFAULT (getdate()) FOR [StartDate]
GO
