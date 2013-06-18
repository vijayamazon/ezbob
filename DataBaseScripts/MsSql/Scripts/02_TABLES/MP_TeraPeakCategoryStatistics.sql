IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_TeraPeakCategoryStatistics]') AND type in (N'U'))
DROP TABLE [dbo].[MP_TeraPeakCategoryStatistics]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_TeraPeakCategoryStatistics](
	[Id] [int] NOT NULL,
	[Listings] [int] NOT NULL,
	[Successful] [int] NOT NULL,
	[ItemsSold] [int] NOT NULL,
	[Revenue] [decimal](18, 4) NOT NULL,
	[SuccessRate] [decimal](18, 4) NOT NULL,
	[OrderItemId] [int] NOT NULL,
	[CategoryId] [int] NOT NULL,
 CONSTRAINT [PK_MP_TeraPeakCategoryStatistics] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
