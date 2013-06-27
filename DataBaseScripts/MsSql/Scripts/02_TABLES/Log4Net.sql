IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Log4Net]') AND type in (N'U'))
DROP TABLE [dbo].[Log4Net]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Log4Net](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Date] [datetime] NOT NULL,
	[Thread] [nvarchar](16) NOT NULL,
	[AppID] [nvarchar](20) NULL,
	[Level] [nvarchar](16) NOT NULL,
	[Logger] [nvarchar](256) NOT NULL,
	[Message] [nvarchar](max) NOT NULL,
	[Exception] [nvarchar](max) NULL,
 CONSTRAINT [PK_Log4Net] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
