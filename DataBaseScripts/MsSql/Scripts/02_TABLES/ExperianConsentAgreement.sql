IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExperianConsentAgreement]') AND type in (N'U'))
DROP TABLE [dbo].[ExperianConsentAgreement]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExperianConsentAgreement](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Template] [nvarchar](max) NOT NULL,
	[CustomerId] [int] NULL,
	[FilePath] [nvarchar](400) NULL,
 CONSTRAINT [PK_ExperianConsentAgreement] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
