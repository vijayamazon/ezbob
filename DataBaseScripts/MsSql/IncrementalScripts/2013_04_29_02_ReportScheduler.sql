IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ReportScheduler]') AND type in (N'U'))
DROP TABLE [dbo].[ReportScheduler]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ReportScheduler](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Type] [nvarchar](200) NULL,
	[Title] [nvarchar](200) NULL,
	[StoredProcedure] [nvarchar](200) NULL,
	[IsDaily] [bit] NULL,
	[IsWeekly] [bit] NULL,
	[IsMonthly] [bit] NULL,
	[Header] [nvarchar](300) NULL,
	[Fields] [nvarchar](300) NULL,
	[ToEmail] [nvarchar](300) NULL,
 CONSTRAINT [PK_ReportScheduler] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
