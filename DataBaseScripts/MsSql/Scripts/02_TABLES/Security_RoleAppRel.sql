IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Security_RoleAppRel]') AND type in (N'U'))
DROP TABLE [dbo].[Security_RoleAppRel]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Security_RoleAppRel](
	[RoleId] [int] NOT NULL,
	[AppId] [int] NOT NULL,
 CONSTRAINT [PK_Security_RoleAppRel] PRIMARY KEY CLUSTERED 
(
	[RoleId] ASC,
	[AppId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
