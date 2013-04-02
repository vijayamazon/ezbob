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