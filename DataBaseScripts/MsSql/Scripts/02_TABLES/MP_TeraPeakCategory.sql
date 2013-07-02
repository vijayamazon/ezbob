IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_TeraPeakCategory]') AND type in (N'U'))
DROP TABLE [dbo].[MP_TeraPeakCategory]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_TeraPeakCategory](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](1000) NOT NULL,
	[FullName] [nvarchar](max) NOT NULL,
	[Level] [int] NOT NULL,
	[ParentCategoryID] [int] NOT NULL,
 CONSTRAINT [PK_MP_TeraPeakCategory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
