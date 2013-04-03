IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CaisReportsHistory]') AND type in (N'U'))
DROP TABLE [dbo].[CaisReportsHistory]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CaisReportsHistory](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Date] [datetime] NULL,
	[FileName] [nvarchar](25) NULL,
	[Type] [int] NULL,
	[OfItems] [int] NULL,
	[GoodUsers] [int] NULL,
	[UploadStatus] [int] NULL,
	[FilePath] [nvarchar](400) NULL,
 CONSTRAINT [PK_CaisReportsHistory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
