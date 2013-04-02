IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_EbayUserAdditionalAccountData]') AND type in (N'U'))
DROP TABLE [dbo].[MP_EbayUserAdditionalAccountData]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_EbayUserAdditionalAccountData](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EbayUserAccountDataId] [int] NOT NULL,
	[Currency] [nvarchar](50) NULL,
	[AccountCode] [nvarchar](256) NULL,
	[Balance] [float] NULL,
 CONSTRAINT [PK_MP_EbayUserAdditionalAccountData] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
