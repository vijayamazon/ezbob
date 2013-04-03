IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Application_NodeSetting]') AND type in (N'U'))
DROP TABLE [dbo].[Application_NodeSetting]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Application_NodeSetting](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[NodeId] [int] NULL,
	[ApplicationId] [bigint] NOT NULL,
	[NODEPOSTFIX] [nvarchar](100) NOT NULL,
	[Name] [nvarchar](150) NOT NULL,
	[Value] [int] NULL,
 CONSTRAINT [PK_Application_NodeSetting] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
