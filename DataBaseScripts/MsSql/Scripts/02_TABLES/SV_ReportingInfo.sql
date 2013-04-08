IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SV_ReportingInfo]') AND type in (N'U'))
DROP TABLE [dbo].[SV_ReportingInfo]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SV_ReportingInfo](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[PathDir] [nvarchar](max) NOT NULL,
	[UserId] [int] NOT NULL,
	[CreationDate] [datetime] NOT NULL,
	[CreatingState] [int] NOT NULL,
	[ErrorMessage] [nvarchar](1000) NULL,
	[TimeCreationCube] [bigint] NOT NULL,
 CONSTRAINT [PK_DirectoryInfo] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
