﻿IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[YodleeBanks]') AND type in (N'U'))
DROP TABLE [dbo].[YodleeBanks]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[YodleeBanks](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](300) NULL,
	[ContentServiceId] [int] NOT NULL,
	[ParentBank] [nvarchar](100) NOT NULL,
	[Active] [bit] NOT NULL,
 CONSTRAINT [PK_YodleeBanks] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[YodleeBanks] ADD  DEFAULT ('abc') FOR [ParentBank]
GO
ALTER TABLE [dbo].[YodleeBanks] ADD  DEFAULT ((1)) FOR [Active]
GO
