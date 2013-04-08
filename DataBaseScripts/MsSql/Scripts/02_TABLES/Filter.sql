IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Filter]') AND type in (N'U'))
DROP TABLE [dbo].[Filter]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Filter](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FilterType] [nvarchar](256) NULL,
	[Status] [nvarchar](256) NULL,
	[Statuses] [nvarchar](2048) NULL,
	[States] [nvarchar](2048) NULL,
	[Name] [nvarchar](256) NULL,
 CONSTRAINT [PK_Filter] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
