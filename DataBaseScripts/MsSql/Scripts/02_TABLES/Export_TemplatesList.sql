IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Export_TemplatesList]') AND type in (N'U'))
DROP TABLE [dbo].[Export_TemplatesList]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Export_TemplatesList](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FileName] [nvarchar](448) NULL,
	[Description] [nvarchar](1024) NULL,
	[VariablesXml] [nvarchar](max) NULL,
	[UploadDate] [datetime] NULL,
	[IsDeleted] [int] NULL,
	[BinaryBody] [varbinary](max) NULL,
	[ExceptionType] [int] NULL,
	[UserId] [int] NULL,
	[DeleterUserId] [int] NULL,
	[DisplayName] [nvarchar](max) NULL,
	[TerminationDate] [datetime] NULL,
	[SignedDocument] [ntext] NULL,
	[DelSignedDocument] [ntext] NULL,
 CONSTRAINT [PK_Export_TemplatesList] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY],
 CONSTRAINT [IX_Export_TemplatesList] UNIQUE NONCLUSTERED 
(
	[FileName] ASC,
	[IsDeleted] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[Export_TemplatesList] ADD  CONSTRAINT [DF_Export_TemplatesList_UploadDate]  DEFAULT (getdate()) FOR [UploadDate]
GO
