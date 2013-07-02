﻿IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Application_Detail]') AND type in (N'U'))
DROP TABLE [dbo].[Application_Detail]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Application_Detail](
	[DetailId] [bigint] IDENTITY(1,1) NOT NULL,
	[ApplicationId] [bigint] NOT NULL,
	[DetailNameId] [int] NULL,
	[ParentDetailId] [bigint] NULL,
	[ValueStr] [nvarchar](max) NULL,
	[ValueNum] [float] NULL,
	[ValueDateTime] [datetime] NULL,
	[IsBinary] [bit] NULL,
 CONSTRAINT [PK_Application_Detail] PRIMARY KEY CLUSTERED 
(
	[DetailId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [i1] ON [dbo].[Application_Detail] 
(
	[ApplicationId] ASC
)
INCLUDE ( [DetailNameId]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [i2] ON [dbo].[Application_Detail] 
(
	[DetailNameId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [i3] ON [dbo].[Application_Detail] 
(
	[ParentDetailId] ASC
)
INCLUDE ( [DetailNameId]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
