IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[hibernate_unique_key]') AND type in (N'U'))
DROP TABLE [dbo].[hibernate_unique_key]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[hibernate_unique_key](
	[next_hi] [int] NULL,
	[InventoryItemIdSeed] [int] NULL
) ON [PRIMARY]
GO
