IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_YodleeTransactionCategories]') AND type in (N'U'))
DROP TABLE [dbo].[MP_YodleeTransactionCategories]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_YodleeTransactionCategories](
	[CategoryId] [nvarchar](3) NOT NULL,
	[Name] [nvarchar](300) NOT NULL,
	[Type] [nvarchar](300) NOT NULL,
 CONSTRAINT [PK_MP_YodleeTransactionCategories] PRIMARY KEY CLUSTERED 
(
	[CategoryId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
