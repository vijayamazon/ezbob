IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_EBayOrderItemDetail]') AND type in (N'U'))
DROP TABLE [dbo].[MP_EBayOrderItemDetail]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_EBayOrderItemDetail](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ItemId] [nvarchar](128) NOT NULL,
	[PrimaryCategoryId] [int] NULL,
	[SecondaryCategoryId] [int] NULL,
	[FreeAddedCategoryId] [int] NULL,
	[Title] [nvarchar](max) NULL,
 CONSTRAINT [PK_MP_EBayOrderItemInfo] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
