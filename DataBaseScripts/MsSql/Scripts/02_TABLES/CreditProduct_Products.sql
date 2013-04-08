IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreditProduct_Products]') AND type in (N'U'))
DROP TABLE [dbo].[CreditProduct_Products]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CreditProduct_Products](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](1024) NULL,
	[Description] [nvarchar](1024) NULL,
	[CreationDate] [datetime] NULL,
	[UserId] [int] NULL,
	[IsDeleted] [int] NULL,
 CONSTRAINT [PK_CreditProduct_Products] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[CreditProduct_Products] ADD  CONSTRAINT [DF_CreditProduct_Products_CreationDate]  DEFAULT (getdate()) FOR [CreationDate]
GO
