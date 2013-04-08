IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EkmConnectorConfigs]') AND type in (N'U'))
DROP TABLE [dbo].[EkmConnectorConfigs]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EkmConnectorConfigs](
	[CfgKey] [varchar](100) NOT NULL,
	[CfgValue] [varchar](100) NULL,
 CONSTRAINT [PK_EkmConnectorConfigs] PRIMARY KEY CLUSTERED 
(
	[CfgKey] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
