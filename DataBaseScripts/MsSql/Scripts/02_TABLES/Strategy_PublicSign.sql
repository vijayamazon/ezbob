IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_PublicSign]') AND type in (N'U'))
DROP TABLE [dbo].[Strategy_PublicSign]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Strategy_PublicSign](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[StrategyPublicId] [int] NOT NULL,
	[StrategyId] [int] NOT NULL,
	[CreationDate] [datetime] NOT NULL,
	[Data] [ntext] NOT NULL,
	[Action] [nvarchar](7) NOT NULL,
	[SignedDocument] [ntext] NOT NULL,
	[UserId] [int] NOT NULL,
	[AllData] [ntext] NOT NULL,
 CONSTRAINT [PK_Strategy_PublicSign] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[Strategy_PublicSign] ADD  CONSTRAINT [DF_Strategy_PublicSign_CreationDate]  DEFAULT (getdate()) FOR [CreationDate]
GO
