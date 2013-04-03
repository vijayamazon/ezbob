IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DataSource_KeyFields]') AND type in (N'U'))
DROP TABLE [dbo].[DataSource_KeyFields]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DataSource_KeyFields](
	[KeyNameId] [int] IDENTITY(1,1) NOT NULL,
	[KeyName] [nvarchar](512) NULL,
 CONSTRAINT [PK_DataSource_KeyFields] PRIMARY KEY CLUSTERED 
(
	[KeyNameId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
