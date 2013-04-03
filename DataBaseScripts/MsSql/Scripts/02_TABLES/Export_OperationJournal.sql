IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Export_OperationJournal]') AND type in (N'U'))
DROP TABLE [dbo].[Export_OperationJournal]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Export_OperationJournal](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[ActionDateTime] [datetime] NOT NULL,
	[SignedDocument] [ntext] NULL,
	[OperationType] [nvarchar](6) NOT NULL,
	[ContentType] [nvarchar](10) NOT NULL,
	[BinaryBody] [varbinary](max) NOT NULL,
	[ContentName] [nvarchar](255) NOT NULL,
	[JournalType] [nvarchar](50) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
