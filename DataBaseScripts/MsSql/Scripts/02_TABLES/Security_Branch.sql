IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Security_Branch]') AND type in (N'U'))
DROP TABLE [dbo].[Security_Branch]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Security_Branch](
	[BranchId] [int] NOT NULL,
	[Name] [nvarchar](255) NULL,
	[Description] [ntext] NULL,
	[Identifier] [nvarchar](255) NULL,
	[CreationDate]  AS (getdate()),
	[ModifyDate]  AS (getdate()),
 CONSTRAINT [PK_SECURITY_BRANCH] PRIMARY KEY CLUSTERED 
(
	[BranchId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
