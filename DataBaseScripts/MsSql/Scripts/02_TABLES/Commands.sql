IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Commands]') AND type in (N'U'))
DROP TABLE [dbo].[Commands]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Commands](
	[Id] [int] NOT NULL,
	[Position] [int] NULL,
	[ListId] [nchar](32) NULL,
	[Type] [nvarchar](128) NOT NULL,
	[SecAppId] [int] NULL,
	[UserId] [int] NULL,
	[AppId] [int] NULL,
	[ParamsXml] [nvarchar](max) NULL,
	[Outparams] [nvarchar](max) NULL,
	[Outlet] [nvarchar](64) NULL,
	[Status] [int] NULL,
	[ControlName] [nvarchar](1024) NULL,
	[OutletName] [nvarchar](1024) NULL,
	[FormName] [nvarchar](1024) NULL,
	[ItemsToBeSigned] [nvarchar](max) NULL,
	[SignatureRequired] [bit] NULL,
	[AttachDescription] [nvarchar](255) NULL,
	[AttachDocType] [nvarchar](255) NULL,
	[AttachFileName] [nvarchar](255) NULL,
	[AttachControlName] [nvarchar](255) NULL,
	[AttachBody] [varbinary](max) NULL,
	[StrategyName] [nvarchar](512) NULL,
	[NodeName] [nvarchar](512) NULL,
 CONSTRAINT [PK_Commands] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
