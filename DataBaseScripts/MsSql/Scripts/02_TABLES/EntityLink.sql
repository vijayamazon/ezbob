IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EntityLink]') AND type in (N'U'))
DROP TABLE [dbo].[EntityLink]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EntityLink](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[SeriaId] [int] NOT NULL,
	[EntityType] [nvarchar](100) NOT NULL,
	[EntityId] [bigint] NOT NULL,
	[UserId] [int] NOT NULL,
	[LinksDoc] [nvarchar](max) NOT NULL,
	[SignedDoc] [nvarchar](max) NULL,
	[IsDeleted] [bit] NULL,
	[IsApproved] [bit] NULL,
 CONSTRAINT [PK_EntityLink] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
