IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FraudCompany]') AND type in (N'U'))
DROP TABLE [dbo].[FraudCompany]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FraudCompany](
	[Id] [int] NOT NULL,
	[CompanyName] [nvarchar](200) NULL,
	[RegistrationNumber] [nvarchar](50) NULL,
	[FraudUserId] [int] NOT NULL,
 CONSTRAINT [PK_FraudCompany] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
