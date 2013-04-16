IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FraudEmailDomain]') AND type in (N'U'))
DROP TABLE [dbo].[FraudEmailDomain]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FraudEmailDomain](
	[Id] [int] NOT NULL,
	[EmailDomain] [nvarchar](250) NULL,
	[FraudUserId] [int] NOT NULL,
 CONSTRAINT [PK_FraudEmailDomain] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
