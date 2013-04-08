IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Application_NodeDataSign]') AND type in (N'U'))
DROP TABLE [dbo].[Application_NodeDataSign]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Application_NodeDataSign](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[applicationId] [bigint] NOT NULL,
	[nodeId] [int] NULL,
	[outletName] [nvarchar](50) NOT NULL,
	[dateAdded] [datetime] NOT NULL,
	[signedData] [ntext] NOT NULL,
	[data] [ntext] NOT NULL,
	[nodeName] [nvarchar](250) NOT NULL,
	[userName] [nvarchar](30) NOT NULL,
 CONSTRAINT [PK_Application_NodeDataSign] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
