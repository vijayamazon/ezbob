IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_AlertDocument]') AND type in (N'U'))
DROP TABLE [dbo].[MP_AlertDocument]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_AlertDocument](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DocName] [nvarchar](500) NULL,
	[UploadDate] [datetime] NULL,
	[UserId] [int] NULL,
	[CustomerId] [int] NULL,
	[Description] [nvarchar](max) NULL,
	[BinaryBody] [varbinary](max) NULL,
 CONSTRAINT [PK_MP_AlertDocument] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
