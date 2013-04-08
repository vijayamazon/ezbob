IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Log_ServiceAction]') AND type in (N'U'))
DROP TABLE [dbo].[Log_ServiceAction]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Log_ServiceAction](
	[LogServiceActionId] [int] IDENTITY(1,1) NOT NULL,
	[Command] [nvarchar](255) NULL,
	[ApplicationId] [int] NULL,
	[Request] [ntext] NULL,
	[Response] [ntext] NULL,
	[DateTime] [datetime] NULL,
	[UserHost] [nvarchar](255) NULL,
 CONSTRAINT [PK_Log_ServiceAction] PRIMARY KEY CLUSTERED 
(
	[LogServiceActionId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[Log_ServiceAction] ADD  CONSTRAINT [DF_Log_ServiceAction_DateTime]  DEFAULT (getdate()) FOR [DateTime]
GO
