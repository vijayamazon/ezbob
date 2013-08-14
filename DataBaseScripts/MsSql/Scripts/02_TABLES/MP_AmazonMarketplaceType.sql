IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_AmazonMarketplaceType]') AND type in (N'U'))
DROP TABLE [dbo].[MP_AmazonMarketplaceType]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_AmazonMarketplaceType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[MarketplaceId] [nvarchar](20) NOT NULL,
	[Country] [nvarchar](50) NULL,
	[Domain] [nvarchar](50) NULL,
 CONSTRAINT [PK_MP_AmazonMarketplaceType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
