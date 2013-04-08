IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MenuItem_Status_Rel]') AND type in (N'U'))
DROP TABLE [dbo].[MenuItem_Status_Rel]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MenuItem_Status_Rel](
	[MenuItemId] [int] NOT NULL,
	[StatusId] [int] NOT NULL,
 CONSTRAINT [PK_MenuItem_Status_Rel] PRIMARY KEY CLUSTERED 
(
	[MenuItemId] ASC,
	[StatusId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
