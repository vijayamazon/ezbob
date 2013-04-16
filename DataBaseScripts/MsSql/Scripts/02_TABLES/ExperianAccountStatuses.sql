IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExperianAccountStatuses]') AND type in (N'U'))
DROP TABLE [dbo].[ExperianAccountStatuses]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExperianAccountStatuses](
	[Id] [varchar](3) NOT NULL,
	[Status] [varchar](10) NULL,
	[DetailedStatus] [varchar](100) NULL,
 CONSTRAINT [PK_ExperianAccountStatuses] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
