IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_EbayAmazonCategory]') AND type in (N'U'))
DROP TABLE [dbo].[MP_EbayAmazonCategory]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_EbayAmazonCategory](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[MarketplaceTypeId] [int] NOT NULL,
	[ParentId] [int] NULL,
	[ServiceCategoryId] [nvarchar](128) NOT NULL,
	[Name] [nvarchar](256) NULL,
	[IsVirtual] [bit] NULL,
 CONSTRAINT [PK_MP_EbayAmazonCategory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_MP_EbayAmazonCategory] ON [dbo].[MP_EbayAmazonCategory] 
(
	[Name] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
