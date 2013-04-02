IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Application_NodeTime]') AND type in (N'U'))
DROP TABLE [dbo].[Application_NodeTime]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Application_NodeTime](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ApplicationId] [int] NULL,
	[NodeId] [int] NULL,
	[UserId] [int] NULL,
	[SecurityApplicationId] [int] NULL,
	[FirstTimeOutage] [int] NULL,
	[OutageTimeLockUnlock] [int] NULL,
	[TimeOfFly] [int] NULL,
	[WorkTime] [int] NULL,
	[ComingTime] [datetime] NULL,
	[ExitTime] [datetime] NULL,
	[ExitType] [int] NULL,
 CONSTRAINT [PK_Application_NodeTime] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Application_NodeTime] ADD  CONSTRAINT [DF_Application_NodeTime_ExitType]  DEFAULT ((0)) FOR [ExitType]
GO
