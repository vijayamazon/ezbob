IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_AmazonOrderItemDetailCatgory]') AND type in (N'U'))
DROP TABLE [dbo].[MP_AmazonOrderItemDetailCatgory]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_AmazonOrderItemDetailCatgory](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AmazonOrderItemDetailId] [int] NOT NULL,
	[EbayAmazonCategoryId] [int] NOT NULL,
 CONSTRAINT [PK_MP_AmazonOrderItemDetailCatgory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
