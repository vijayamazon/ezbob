IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CustomerStatuses]') AND type in (N'U'))
DROP TABLE [dbo].[CustomerStatuses]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustomerStatuses](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](100) NULL,
	[IsEnabled] [bit] NOT NULL,
 CONSTRAINT [PK_CustomerStatuses] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[CustomerStatuses] ADD  DEFAULT ((0)) FOR [IsEnabled]
GO
