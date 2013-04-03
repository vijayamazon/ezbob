IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreditProduct_Params]') AND type in (N'U'))
DROP TABLE [dbo].[CreditProduct_Params]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CreditProduct_Params](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](1024) NULL,
	[Type] [nvarchar](1024) NULL,
	[Description] [nvarchar](1024) NULL,
	[CreditProductId] [int] NULL,
	[Value] [nvarchar](2096) NULL,
 CONSTRAINT [PK_CreditProduct_Params] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
