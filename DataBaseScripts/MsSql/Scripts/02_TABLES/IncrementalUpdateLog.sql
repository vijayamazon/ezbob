IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IncrementalUpdateLog]') AND type in (N'U'))
DROP TABLE [dbo].[IncrementalUpdateLog]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[IncrementalUpdateLog](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[HostName] [nvarchar](50) NOT NULL,
	[AccountName] [nvarchar](50) NOT NULL,
	[ActionDate] [datetime] NOT NULL,
	[FileName] [nvarchar](200) NOT NULL,
	[FileBody] [nvarchar](max) NOT NULL,
	[CheckSum] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_IncrementalUpdateLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
