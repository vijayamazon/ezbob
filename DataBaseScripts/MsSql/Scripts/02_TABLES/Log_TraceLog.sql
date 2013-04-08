IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Log_TraceLog]') AND type in (N'U'))
DROP TABLE [dbo].[Log_TraceLog]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Log_TraceLog](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ApplicationId] [int] NULL,
	[Type] [tinyint] NULL,
	[Code] [int] NULL,
	[Message] [ntext] NULL,
	[Data] [ntext] NULL,
	[InsertDate] [datetime] NULL,
	[ThreadId] [nvarchar](50) NULL,
 CONSTRAINT [PK_Log_TraceLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
