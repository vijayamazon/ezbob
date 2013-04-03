IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TaskedStrategyParams]') AND type in (N'U'))
DROP TABLE [dbo].[TaskedStrategyParams]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TaskedStrategyParams](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TSId] [int] NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
	[DisplayName] [nvarchar](256) NOT NULL,
	[Description] [nvarchar](1024) NULL,
	[TypeName] [nvarchar](64) NOT NULL,
	[InitialValue] [nvarchar](256) NOT NULL,
	[ConstraintString] [nvarchar](1024) NULL,
 CONSTRAINT [PK_TSPARAM] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
