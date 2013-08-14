IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ReportArguments]') AND type in (N'U'))
DROP TABLE [dbo].[ReportArguments]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ReportArguments](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ReportArgumentNameId] [int] NOT NULL,
	[ReportId] [int] NOT NULL
) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX [IDX_ReportArgument] ON [dbo].[ReportArguments] 
(
	[ReportArgumentNameId] ASC,
	[ReportId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
