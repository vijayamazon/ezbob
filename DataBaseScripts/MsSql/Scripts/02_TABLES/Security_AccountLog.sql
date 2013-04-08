IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Security_AccountLog]') AND type in (N'U'))
DROP TABLE [dbo].[Security_AccountLog]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Security_AccountLog](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EventDate] [datetime] NULL,
	[EventType] [int] NULL,
	[UserId] [int] NULL,
	[Data] [nvarchar](2048) NULL,
 CONSTRAINT [PK_Security_AccountLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Security_AccountLog] ADD  CONSTRAINT [DF_Security_AccountLog_EventDate]  DEFAULT (getdate()) FOR [EventDate]
GO
