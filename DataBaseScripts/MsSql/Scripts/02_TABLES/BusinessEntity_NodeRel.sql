IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BusinessEntity_NodeRel]') AND type in (N'U'))
DROP TABLE [dbo].[BusinessEntity_NodeRel]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BusinessEntity_NodeRel](
	[Id] [int] NOT NULL,
	[NodeId] [int] NOT NULL,
	[BusinessEntityId] [int] NOT NULL,
 CONSTRAINT [PK_BusinessEntity_NodeRel] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
