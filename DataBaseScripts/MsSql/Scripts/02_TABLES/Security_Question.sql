IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Security_Question]') AND type in (N'U'))
DROP TABLE [dbo].[Security_Question]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Security_Question](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[name] [varchar](200) NULL
) ON [PRIMARY]
GO
