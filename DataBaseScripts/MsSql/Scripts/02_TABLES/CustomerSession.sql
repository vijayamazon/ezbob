IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CustomerSession]') AND type in (N'U'))
DROP TABLE [dbo].[CustomerSession]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustomerSession](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerId] [int] NOT NULL,
	[StartSession] [datetime] NOT NULL,
	[Ip] [nvarchar](50) NOT NULL,
	[IsPasswdOk] [bit] NOT NULL,
	[ErrorMessage] [nvarchar](50) NULL,
 CONSTRAINT [PK_CustomerSession] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
