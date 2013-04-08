IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Application_Suspended]') AND type in (N'U'))
DROP TABLE [dbo].[Application_Suspended]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Application_Suspended](
	[ApplicationId] [bigint] NOT NULL,
	[ExecutionState] [nvarchar](max) NULL,
	[Postfix] [nvarchar](512) NULL,
	[Target] [nvarchar](50) NOT NULL,
	[Label] [nvarchar](250) NOT NULL,
	[Message] [varbinary](max) NOT NULL,
	[AppSpecific] [int] NULL,
	[Date] [datetime] NOT NULL,
	[ExecutionType] [smallint] NULL,
	[ExecutionPathCurrentItemId] [int] NOT NULL,
 CONSTRAINT [PK_Application_Suspended] PRIMARY KEY CLUSTERED 
(
	[ApplicationId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
