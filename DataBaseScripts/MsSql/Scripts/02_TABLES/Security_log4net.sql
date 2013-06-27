IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Security_log4net]') AND type in (N'U'))
DROP TABLE [dbo].[Security_log4net]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Security_log4net](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Message] [nvarchar](max) NULL,
	[EventDate] [datetime] NULL,
	[EventJournal] [int] NULL,
	[EventType] [int] NULL,
	[UserName] [nvarchar](1024) NULL,
	[Level] [nvarchar](50) NULL,
	[Thread] [nvarchar](16) NULL,
	[AppID] [nvarchar](20) NULL,
	[Logger] [nvarchar](256) NULL,
	[Exception] [nvarchar](max) NULL,
 CONSTRAINT [PK_Security_log4net] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[Security_log4net] ADD  CONSTRAINT [DF_Security_log4net_EventDate]  DEFAULT (getdate()) FOR [EventDate]
GO
