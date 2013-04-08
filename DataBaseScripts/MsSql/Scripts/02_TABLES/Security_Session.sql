IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Security_Session]') AND type in (N'U'))
DROP TABLE [dbo].[Security_Session]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Security_Session](
	[UserId] [int] NOT NULL,
	[AppId] [int] NOT NULL,
	[State] [tinyint] NOT NULL,
	[SessionId] [nvarchar](32) NOT NULL,
	[CreationDate] [datetime] NOT NULL,
	[LastAccessTime] [datetime] NOT NULL,
	[HostAddress] [nvarchar](max) NULL,
 CONSTRAINT [PK_Security_Session] PRIMARY KEY CLUSTERED 
(
	[SessionId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Security_Session] ADD  CONSTRAINT [DF_AppSession_State]  DEFAULT ((0)) FOR [State]
GO
ALTER TABLE [dbo].[Security_Session] ADD  CONSTRAINT [DF_AppSession_CreationDate]  DEFAULT (getdate()) FOR [CreationDate]
GO
